using FeedCord.src.Infrastructure.Http;
using FeedCord.src.Services.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace FeedCord.src.Infrastructure.Parsers
{
    public class ImageParserService : IImageParserService
    {
        private readonly ICustomHttpClient httpClient;
        private readonly ILogger<ImageParserService> logger;

        public ImageParserService(ICustomHttpClient httpClient, ILogger<ImageParserService> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public async Task<string> TryExtractImageLink(string pageUrl)
        {
            if (string.IsNullOrEmpty(pageUrl))
                return string.Empty;

            try
            {
                var response = await httpClient.GetAsyncWithFallback(pageUrl);
                response.EnsureSuccessStatusCode();

                var htmlContent = await response.Content.ReadAsStringAsync();

                var doc = new HtmlDocument();
                doc.LoadHtml(htmlContent);

                string foundUrl =
                    GetMetaTagContent(doc, "property", "og:image")
                    ?? GetMetaTagContent(doc, "name", "twitter:image")
                    ?? GetLinkRel(doc, "image_src")
                    ?? GetFirstImg(doc);

                if (string.IsNullOrEmpty(foundUrl))
                {
                    logger?.LogInformation("No image found via OG, Twitter, image_src, or first <img> for {Url}", pageUrl);
                    return string.Empty;
                }

                return MakeAbsoluteUrl(pageUrl, foundUrl);
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Error extracting image URL from page: {Url}", pageUrl);
                return string.Empty;
            }
        }
        private static string GetMetaTagContent(HtmlDocument doc, string key, string valueMatch)
        {
            var node = doc.DocumentNode
                .SelectSingleNode($"//meta[@{key}='{valueMatch}']");

            return node?.GetAttributeValue("content", null);
        }
        private static string GetLinkRel(HtmlDocument doc, string relValue)
        {
            var node = doc.DocumentNode
                .SelectSingleNode($"//link[@rel='{relValue}']");

            return node?.GetAttributeValue("href", null);
        }
        private static string GetFirstImg(HtmlDocument doc)
        {
            var imgNode = doc.DocumentNode.SelectSingleNode("//img");
            return imgNode?.GetAttributeValue("src", null);
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
