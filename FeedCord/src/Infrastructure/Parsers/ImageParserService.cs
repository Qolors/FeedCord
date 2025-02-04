using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using FeedCord.src.Infrastructure.Http;
using FeedCord.src.Services.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace FeedCord.src.Infrastructure.Parsers
{
    public class ImageParserService : IImageParserService
    {
        private readonly ICustomHttpClient _httpClient;
        private readonly ILogger<ImageParserService> _logger;

        public ImageParserService(ICustomHttpClient httpClient, ILogger<ImageParserService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> TryExtractImageLink(string pageUrl, string xmlSource)
        {
            // 1) Try to extract an image directly from the feed XML
            if (!string.IsNullOrWhiteSpace(xmlSource))
            {
                var feedImageUrl = ExtractImageFromFeedXml(xmlSource);
                if (IsValidImageUrl(feedImageUrl))
                {
                    // Make sure it's absolute
                    feedImageUrl = MakeAbsoluteUrl(pageUrl, feedImageUrl);
                    return feedImageUrl;
                }
            }

            // 2) If no valid image found in feed, do the existing fallback to web scraping
            return await ScrapeImageFromWebpage(pageUrl);
        }

        private static string ExtractImageFromFeedXml(string xmlSource)
        {
            try
            {
                var xdoc = XDocument.Parse(xmlSource);

                // Try #1: <enclosure url="..." type="image/..." />
                var enclosureImage = xdoc.Descendants("enclosure")
                    .FirstOrDefault(e => e.Attribute("type") != null &&
                                         e.Attribute("type").Value.StartsWith("image/", StringComparison.OrdinalIgnoreCase));
                if (enclosureImage != null)
                {
                    var url = enclosureImage.Attribute("url")?.Value;
                    if (!string.IsNullOrWhiteSpace(url)) return url;
                }

                // Try #2: <media:content url="..." type="image/..."> or <media:thumbnail url="...">
                // Note that the media: namespace might need to be accounted for if declared in the feed
                // For simplicity, we ignore namespaces here using LocalName checks
                var mediaContent = xdoc.Descendants()
                    .FirstOrDefault(el =>
                        (el.Name.LocalName == "content" || el.Name.LocalName == "thumbnail") &&
                        el.Attributes("url").Any() &&
                        (el.Attribute("type")?.Value?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ?? true)
                    );
                if (mediaContent != null)
                {
                    var url = mediaContent.Attribute("url")?.Value;
                    if (!string.IsNullOrWhiteSpace(url)) return url;
                }

                // Try #3: <itunes:image href="..." />
                var itunesImage = xdoc.Descendants().FirstOrDefault(el => el.Name.LocalName == "image" &&
                                                                          el.Name.NamespaceName.Contains("itunes") &&
                                                                          el.Attribute("href") != null);
                if (itunesImage != null)
                {
                    var url = itunesImage.Attribute("href")?.Value;
                    if (!string.IsNullOrWhiteSpace(url)) return url;
                }

                // Try #4: Look for <description> or <content:encoded> that might contain an <img> tag
                var descNode = xdoc.Descendants("description").FirstOrDefault();
                var contentNode = xdoc.Descendants().FirstOrDefault(n => n.Name.LocalName == "encoded");

                // Each might contain HTML with an <img> tag
                var descHtml = descNode?.Value ?? string.Empty;
                var contentHtml = contentNode?.Value ?? string.Empty;

                var fromDesc = ExtractImgFromHtml(descHtml);
                if (!string.IsNullOrEmpty(fromDesc)) return fromDesc;

                var fromContent = ExtractImgFromHtml(contentHtml);
                if (!string.IsNullOrEmpty(fromContent)) return fromContent;
            }
            catch (Exception ex)
            {
                // Log or handle parse exceptions
                Console.WriteLine("Failed to parse feed XML. " + ex.Message);
            }

            // Nothing found in feed
            return string.Empty;
        }

        private static string ExtractImgFromHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Just get the first <img src="...">
            var imgNode = doc.DocumentNode.SelectSingleNode("//img[@src]");
            if (imgNode != null)
            {
                var src = imgNode.GetAttributeValue("src", null);
                if (!string.IsNullOrWhiteSpace(src))
                {
                    return src;
                }
            }
            return string.Empty;
        }

        private async Task<string> ScrapeImageFromWebpage(string pageUrl)
        {
            if (string.IsNullOrWhiteSpace(pageUrl))
                return string.Empty;

            try
            {
                // Download HTML
                var response = await _httpClient.GetAsyncWithFallback(pageUrl);
                response.EnsureSuccessStatusCode();
                var htmlContent = await response.Content.ReadAsStringAsync();

                // Existing logic
                var doc = new HtmlDocument();
                doc.LoadHtml(htmlContent);

                var imageUrl = ExtractImageFromDocument(doc);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    imageUrl = MakeAbsoluteUrl(pageUrl, imageUrl);
                    return imageUrl;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error scraping image from webpage: {PageUrl}", pageUrl);
            }

            return string.Empty;
        }

        private static string ExtractImageFromDocument(HtmlDocument doc)
        {
            var imageUrl = GetMetaTagContent(doc, "property", "og:image");
            if (IsValidImageUrl(imageUrl))
                return imageUrl;

            imageUrl = GetMetaTagContent(doc, "property", "og:image:secure_url");
            if (IsValidImageUrl(imageUrl))
                return imageUrl;

            imageUrl = GetMetaTagContent(doc, "name", "twitter:image");
            if (IsValidImageUrl(imageUrl))
                return imageUrl;

            imageUrl = GetLinkRel(doc, "image_src");
            if (IsValidImageUrl(imageUrl))
                return imageUrl;

            imageUrl = GetFirstImageWithAttribute(doc, "data-src");
            if (IsValidImageUrl(imageUrl))
                return imageUrl;

            imageUrl = GetElementById(doc, "post-image");
            if (IsValidImageUrl(imageUrl))
                return imageUrl;

            imageUrl = GetFirstImg(doc);
            if (IsValidImageUrl(imageUrl))
                return imageUrl;

            return string.Empty;
        }

        private static bool IsValidImageUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                return false;

            if (Uri.TryCreate(url, UriKind.Absolute, out var parsedUri))
            {
                if (parsedUri.Scheme.Equals("file", StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        private static string GetMetaTagContent(HtmlDocument doc, string attributeKey, string attributeValue)
        {
            var metaNode = doc.DocumentNode.SelectSingleNode($"//meta[@{attributeKey}='{attributeValue}']");
            return metaNode?.GetAttributeValue("content", null);
        }

        private static string GetLinkRel(HtmlDocument doc, string relValue)
        {
            var linkNode = doc.DocumentNode.SelectSingleNode($"//link[@rel='{relValue}']");
            return linkNode?.GetAttributeValue("href", null);
        }

        private static string GetFirstImg(HtmlDocument doc)
        {
            var imgNode = doc.DocumentNode.SelectSingleNode("//img[@src]");
            return imgNode?.GetAttributeValue("src", null);
        }

        private static string GetFirstImageWithAttribute(HtmlDocument doc, string attributeName)
        {
            var imgNode = doc.DocumentNode.SelectSingleNode($"//img[@{attributeName}]");
            return imgNode?.GetAttributeValue(attributeName, null);
        }

        private static string GetElementById(HtmlDocument doc, string elementId)
        {
            var node = doc.GetElementbyId(elementId);
            if (node != null)
            {
                var src = node.GetAttributeValue("src", null);
                if (!string.IsNullOrWhiteSpace(src))
                    return src;
                src = node.GetAttributeValue("data-src", null);
                return src;
            }
            return null;
        }
        private static string MakeAbsoluteUrl(string pageUrl, string foundUrl)
        {
            if (Uri.TryCreate(foundUrl, UriKind.Absolute, out var absolute))
            {
                return absolute.ToString();
            }

            if (Uri.TryCreate(pageUrl, UriKind.Absolute, out var baseUri) &&
                Uri.TryCreate(baseUri, foundUrl, out var relativeUri))
            {
                return relativeUri.ToString();
            }

            return foundUrl;
        }
    }
}
