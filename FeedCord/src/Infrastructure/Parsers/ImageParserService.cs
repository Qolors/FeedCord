using System;
using System.Threading.Tasks;
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

        /// <summary>
        /// Attempts to download the page at <paramref name="pageUrl"/>, parse the HTML,
        /// and return the first valid image URL it finds (making it absolute if necessary).
        /// </summary>
        public async Task<string> TryExtractImageLink(string pageUrl)
        {
            if (string.IsNullOrWhiteSpace(pageUrl))
                return string.Empty;

            try
            {
                // Download the HTML content (using a custom HTTP client that may have fallback logic)
                var response = await _httpClient.GetAsyncWithFallback(pageUrl);
                response.EnsureSuccessStatusCode();
                var htmlContent = await response.Content.ReadAsStringAsync();

                var doc = new HtmlDocument();
                doc.LoadHtml(htmlContent);

                // Try multiple extraction strategies in order.
                var imageUrl = ExtractImageFromDocument(doc);
                if (string.IsNullOrEmpty(imageUrl))
                {
                    _logger.LogInformation("No image found in page {PageUrl}", pageUrl);
                    return string.Empty;
                }

                // Convert relative URLs to absolute ones.
                imageUrl = MakeAbsoluteUrl(pageUrl, imageUrl);
                return imageUrl;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error extracting image URL from page: {PageUrl}", pageUrl);
                return string.Empty;
            }
        }

        /// <summary>
        /// Tries several strategies to find an image URL from the HTML document.
        /// </summary>
        private static string ExtractImageFromDocument(HtmlDocument doc)
        {
            // Strategy 1: Open Graph (og:image)
            var imageUrl = GetMetaTagContent(doc, "property", "og:image");
            if (IsValidImageUrl(imageUrl))
                return imageUrl;

            // Strategy 2: Secure Open Graph (og:image:secure_url)
            imageUrl = GetMetaTagContent(doc, "property", "og:image:secure_url");
            if (IsValidImageUrl(imageUrl))
                return imageUrl;

            // Strategy 3: Twitter image (twitter:image)
            imageUrl = GetMetaTagContent(doc, "name", "twitter:image");
            if (IsValidImageUrl(imageUrl))
                return imageUrl;

            // Strategy 4: link rel image_src (often used by some feeds)
            imageUrl = GetLinkRel(doc, "image_src");
            if (IsValidImageUrl(imageUrl))
                return imageUrl;

            // Strategy 5: Look for a lazy-loaded image (using data-src)
            imageUrl = GetFirstImageWithAttribute(doc, "data-src");
            if (IsValidImageUrl(imageUrl))
                return imageUrl;

            // Strategy 6: Reddit-specific markup (e.g. <img id="post-image">)
            imageUrl = GetElementById(doc, "post-image");
            if (IsValidImageUrl(imageUrl))
                return imageUrl;

            // Strategy 7: Fallback – use the first <img> tag with a src attribute
            imageUrl = GetFirstImg(doc);
            if (IsValidImageUrl(imageUrl))
                return imageUrl;

            return string.Empty;
        }

        /// <summary>
        /// Returns true if the URL is nonempty and does not appear to be a data or javascript URL.
        /// </summary>
        private static bool IsValidImageUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            // Exclude inline images and scripts
            if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        /// <summary>
        /// Finds a meta tag by a given key and value, then returns its content.
        /// </summary>
        private static string GetMetaTagContent(HtmlDocument doc, string attributeKey, string attributeValue)
        {
            var metaNode = doc.DocumentNode.SelectSingleNode($"//meta[@{attributeKey}='{attributeValue}']");
            if (metaNode != null)
            {
                var content = metaNode.GetAttributeValue("content", null);
                if (!string.IsNullOrWhiteSpace(content))
                    return content;
            }
            return null;
        }

        /// <summary>
        /// Returns the href attribute of the first link element with the given rel value.
        /// </summary>
        private static string GetLinkRel(HtmlDocument doc, string relValue)
        {
            var linkNode = doc.DocumentNode.SelectSingleNode($"//link[@rel='{relValue}']");
            return linkNode?.GetAttributeValue("href", null);
        }

        /// <summary>
        /// Returns the src attribute of the first image tag found.
        /// </summary>
        private static string GetFirstImg(HtmlDocument doc)
        {
            var imgNode = doc.DocumentNode.SelectSingleNode("//img[@src]");
            return imgNode?.GetAttributeValue("src", null);
        }

        /// <summary>
        /// Looks for the first image element with the specified attribute (e.g. data-src).
        /// </summary>
        private static string GetFirstImageWithAttribute(HtmlDocument doc, string attributeName)
        {
            var imgNode = doc.DocumentNode.SelectSingleNode($"//img[@{attributeName}]");
            return imgNode?.GetAttributeValue(attributeName, null);
        }

        /// <summary>
        /// Searches for an element by ID (e.g. "post-image") and returns its src or data-src.
        /// </summary>
        private static string GetElementById(HtmlDocument doc, string elementId)
        {
            var node = doc.GetElementbyId(elementId);
            if (node != null)
            {
                // Try the standard src attribute first.
                var src = node.GetAttributeValue("src", null);
                if (!string.IsNullOrWhiteSpace(src))
                    return src;
                // Fallback to data-src if available.
                src = node.GetAttributeValue("data-src", null);
                return src;
            }
            return null;
        }

        /// <summary>
        /// Converts a (possibly relative) image URL to an absolute URL based on the page URL.
        /// </summary>
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
