using Agility.Web;
using Agility.Web.Objects;
using {YOURAPP}.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace {YOURAPP}.Controllers
{
    public class SitemapController : Controller
    {
        private const string XMLNSAttribute = "http://www.sitemaps.org/schemas/sitemap/0.9";
        private const string XMLNSImageAttribute = "http://www.google.com/schemas/sitemap-image/1.1";


        public ActionResult XmlSitemap()
        {

            ContentResult result = new ContentResult();

            // Set the content-type			
            result.ContentType = "text/xml";
            result.ContentEncoding = Encoding.UTF8;

            StringWriter sw = new StringWriter();
            XmlTextWriter writer = new XmlTextWriter(sw);
            writer.Formatting = Formatting.Indented;

            writer.WriteRaw("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            // write out <PromoItem>s
            writer.WriteStartElement("urlset");
            writer.WriteAttributeString("xmlns", XMLNSAttribute);
            writer.WriteAttributeString("xmlns:image", XMLNSImageAttribute);
            // write elements

            BuildSiteMap(ControllerContext, writer);

            // write out </PromoItems>
            writer.WriteEndElement();

            writer.Close();

            result.Content = sw.ToString();

            return result;


        }


        private void BuildSiteMap(ControllerContext context, XmlTextWriter writer)
        {
            //Loops through All Languages
            string startingLanguage = Agility.Web.AgilityContext.LanguageCode;
            Agility.Web.Objects.Language[] langs = Agility.Web.AgilityContext.Domain.Languages;


            var channel = AgilityContext.CurrentChannel;

            foreach (Agility.Web.Objects.Language l in langs)
            {

                var channelDomain = channel.Domains.FirstOrDefault(d =>
                    string.Equals(d.DefaultLanguage, l.LanguageCode, StringComparison.CurrentCultureIgnoreCase)
                    && d.ForceDefaultLanguageToThisDomain);

                Agility.Web.AgilityContext.LanguageCode = l.LanguageCode;



                Agility.Web.Providers.AgilitySiteMapProvider oProvider = new Agility.Web.Providers.AgilitySiteMapProvider();
                foreach (System.Web.SiteMapNode oNode in oProvider.RootNode.GetAllNodes())
                {

                    WriteURLNode(context, writer, l.LanguageCode, oNode, channelDomain);
                }
            }

            Agility.Web.AgilityContext.LanguageCode = startingLanguage;
        }

        private void WriteURLNode(ControllerContext context, XmlTextWriter writer, string langCode, System.Web.SiteMapNode node, ChannelDomain channelDomain)
        {
            //skip stuff that doesn't belong on the sitemap...
            if (string.Equals(node["SitemapVisible"], "false", StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }

            string host = null;
            if (channelDomain == null)
            {
                //no default domain per language... 
                host = context.HttpContext.Request.Url.ToString();
                host = host.Substring(0, host.IndexOf("/", host.IndexOf("://") + 3));
            }
            else
            {
                host = channelDomain.URL.TrimEnd('/');

            }

            string loc = node.Url.ToLowerInvariant();
            if (loc.EndsWith(".aspx"))
            {
                loc = loc.Substring(0, loc.LastIndexOf(".aspx"));
            }

            DateTime lastMod = DateTime.MinValue;
            string changeFreq = "daily";
            double priority = 0;
            List<SiteMapImage> images = new List<SiteMapImage>();

            Agility.Web.Objects.AgilitySiteMapNode agilityNode = node as Agility.Web.Objects.AgilitySiteMapNode;
            Agility.Web.Objects.AgilityDynamicSiteMapNode dynamicNode = node as Agility.Web.Objects.AgilityDynamicSiteMapNode;

            if (loc == "~/home" || loc == "~/default")
            {
                //home page
                loc = "/";
                changeFreq = "daily";

                //home page has highest priority
                priority = 1.0;

            }
            else if (dynamicNode != null)
            {
                //get the item that this page represents and use the last mod of it...
                AgilityContentRepository<AgilityContentItem> content = new AgilityContentRepository<AgilityContentItem>(dynamicNode.ReferenceName);
                AgilityContentItem item = content.Item(string.Format("ContentID = {0}", dynamicNode.ContentID));
                lastMod = item.ModifiedDate;
            }
            else if (agilityNode != null)
            {
                if (agilityNode.AgilityPage != null)
                {
                    foreach (var section in agilityNode.AgilityPage.ContentSections)
                    {
                        AgilityContentRepository<AgilityContentItem> content = new AgilityContentRepository<AgilityContentItem>(section.ContentReferenceName);
                        var item = content.Items().FirstOrDefault();

                        if (item != null && item.ModifiedDate > lastMod) lastMod = item.ModifiedDate;

                    }
                }
            }

            if (loc.Contains("javascript:void(0)"))
            {
                return;
            }

            writer.WriteStartElement("url");
            if (!string.IsNullOrEmpty(loc))
            {
                writer.WriteStartElement("loc");

                if (loc.StartsWith("http"))
                {
                    writer.WriteString(loc);
                }
                else
                {
                    writer.WriteString(string.Format("{0}{1}", host, ResolveUrl(loc).ToLowerInvariant()));
                }
                writer.WriteEndElement();
            }


            if (lastMod > DateTime.MinValue)
            {
                writer.WriteStartElement("lastmod");
                writer.WriteString(lastMod.ToUniversalTime().ToString("u").Replace(" ", "T"));
                writer.WriteEndElement();


                if (loc == "/")
                {

                }
                else
                {
                    //subtract a tenth for each 

                    int weeks = (int)Math.Floor((DateTime.Now - lastMod).TotalDays / 7);

                    priority = 1.0 - (weeks / .1);
                    if (priority < 0) priority = 0;
                }
            }

            if (priority > 0)
            {
                writer.WriteStartElement("priority");
                writer.WriteString(priority.ToString("F1"));
                writer.WriteEndElement();
            }

            if (!string.IsNullOrEmpty(changeFreq))
            {
                writer.WriteStartElement("changefreq");
                writer.WriteString(changeFreq);
                writer.WriteEndElement();
            }


            writer.WriteEndElement();
        }
 
        static string ResolveUrl(string relativeUrl)
        {
            if (relativeUrl.StartsWith("~/")
                && !relativeUrl.StartsWith("~/" + AgilityContext.LanguageCode, StringComparison.InvariantCultureIgnoreCase)
                && AgilityContext.IsUsingLanguageModule
                && relativeUrl.IndexOf(".aspx", StringComparison.InvariantCultureIgnoreCase) != -1)
            {

                relativeUrl = "~/" + AgilityContext.LanguageCode + relativeUrl.Substring(1);
            }

            //replace the ~/
            if (relativeUrl.StartsWith("~/"))
            {

                string appPath = "/";
                if (appPath.EndsWith("/")) return string.Format("{0}{1}", appPath, relativeUrl.Substring(2));
                return string.Format("{0}{1}", appPath, relativeUrl.Substring(1));

            }

            return relativeUrl;
        }


    }
}
