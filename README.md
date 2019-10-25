# SitemapController-NETFramework
Sample code demonstrating how you can generate a sitemap.xml file based on your Agility CMS pages. This is for use with `Agility.Web` for a .NET Framework MVC website.

## Usage
1. In your ASP.NET MVC project, create a Controller Action Result in your website and name it `SitemapController.cs'.
2. Copy and paste the contents of [SitemapController.cs](https://github.com/agility/SitemapController-NETFramework/blob/master/SitemapController.cs) in your controller you just created.
3. In your `~/App_Start/RouteCongig.cs` file, add in the following route mapping:
```
routes.MapRoute("Sitemap", "sitemap.xml", new { controller = "Sitemap", action = "XmlSitemap" });
```

## Test
Run your site locally and navigate to `/sitemap.xml` and you should have XML output that looks something like:
```
<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9" xmlns:image="http://www.google.com/schemas/sitemap-image/1.1">
  <url>
    <loc>http://localhost/</loc>
    <priority>1.0</priority>
    <changefreq>daily</changefreq>
  </url>
  <url>
    <loc>http://localhost/customer-gallery</loc>
    <lastmod>2016-01-14T17:52:25Z</lastmod>
    <changefreq>daily</changefreq>
  </url>
  <url>
    <loc>http://localhost/products</loc>
    <lastmod>2019-01-18T14:04:23Z</lastmod>
    <changefreq>daily</changefreq>
  </url>
  <url>
    <loc>http://localhost/contact-us</loc>
    <lastmod>2019-08-19T20:32:54Z</lastmod>
    <changefreq>daily</changefreq>
  </url>
</urlset>
```
