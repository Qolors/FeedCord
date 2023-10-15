using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace FeedCord.src.Services
{
    internal class RssProcessorService : IRssProcessorService
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<RssProcessorService> logger;

        public RssProcessorService(IHttpClientFactory httpClientFactory, ILogger<RssProcessorService> logger)
        {
            this.logger = logger;
            this.httpClient = httpClientFactory.CreateClient();
        }

        public async Task<Post> ParseRssFeedAsync(string xmlContent)
        {
            var xDoc = XDocument.Parse(xmlContent);

            string subtitle = xDoc.Descendants("title").FirstOrDefault().Value;

            var latestPost = xDoc.Descendants("item").FirstOrDefault();

            if (latestPost == null)
            {
                logger.LogError("No items found in the RSS feed");
                return null;
            }

            string rawDescription = latestPost?.Element("description")?.Value ?? string.Empty;
            string rawTitle = latestPost?.Element("title")?.Value ?? string.Empty;

            string description = StripTags(rawDescription);
            string title = StripTags(rawTitle);
            string imageLink = await ExtractUrls(latestPost?.Element("link")?.Value ?? string.Empty);

            if (description.Length > 150)
            {
                description = description.Substring(0, 147) + "...";
            }

            return new Post(
                title,
                imageLink,
                description,
                latestPost?.Element("link")?.Value ?? string.Empty,
                subtitle,
                DateTime.TryParse(latestPost?.Element("pubDate")?.Value, out var pubDate) ? pubDate : default
            );
        }

        public string StripTags(string source)
        {
            return Regex.Replace(source, "<.*?>", string.Empty);
        }

        public async Task<string> ExtractUrls(string source)
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(source);
                response.EnsureSuccessStatusCode();
                string htmlContent = await response.Content.ReadAsStringAsync();

                var htmlDocument = new HtmlAgilityPack.HtmlDocument();
                htmlDocument.LoadHtml(htmlContent);

                var ogImage = htmlDocument.DocumentNode.SelectSingleNode("//meta[@property='og:image']")?.GetAttributeValue("content", string.Empty);

                return ogImage ?? string.Empty;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error extracting URL from source: {Source}", source);
                return string.Empty;
            }
        }
    }
}
