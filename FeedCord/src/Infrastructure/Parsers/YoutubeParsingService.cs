using FeedCord.src.Common;
using FeedCord.src.Infrastructure.Http;
using FeedCord.src.Services.Interfaces;
using System.Xml.Linq;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace FeedCord.src.Infrastructure.Parsers
{
    public class YoutubeParsingService : IYoutubeParsingService
    {
        private ICustomHttpClient httpClient;
        private ILogger<YoutubeParsingService> logger;
        public YoutubeParsingService(ICustomHttpClient httpClient, ILogger<YoutubeParsingService> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public async Task<Post?> GetXmlUrlAndFeed(string xml)
        {
            logger.LogInformation("Parsing XML to find RSS feed link.");

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(xml);

            var node = doc.DocumentNode.SelectSingleNode("//link[@rel='alternate' and @type='application/rss+xml']");

            if (node != null)
            {
                var hrefValue = node.GetAttributeValue("href", "");
                logger.LogInformation($"Found RSS feed URL: {hrefValue}");
                return await GetRecentPost(hrefValue);
            }

            logger.LogInformation("No RSS feed link found in the provided XML.");
            return null;
        }

        private async Task<Post?> GetRecentPost(string xmlUrl)
        {
            if (string.IsNullOrEmpty(xmlUrl))
            {
                logger.LogInformation("Provided XML URL is null or empty.");
                return null;
            }

            try
            {
                HttpResponseMessage response = await httpClient.GetAsyncWithFallback(xmlUrl);
                response.EnsureSuccessStatusCode();

                string xmlContent = await response.Content.ReadAsStringAsync();
                logger.LogInformation("Successfully retrieved RSS feed.");

                XDocument xdoc = XDocument.Parse(xmlContent);
                XNamespace atomNs = "http://www.w3.org/2005/Atom";
                XNamespace mediaNs = "http://search.yahoo.com/mrss/";

                var channelTitle = xdoc.Root.Element(atomNs + "title")?.Value ?? string.Empty;
                var videoEntry = xdoc.Root.Element(atomNs + "entry");

                if (videoEntry is null)
                {
                    logger.LogInformation("No recent post found in RSS feed.");
                    return null;
                }

                var videoTitle = videoEntry.Element(atomNs + "title")?.Value ?? string.Empty;
                var videoLink = videoEntry.Element(atomNs + "link")?.Attribute("href")?.Value ?? string.Empty;
                var videoThumbnail = videoEntry.Element(mediaNs + "group")?.Element(mediaNs + "thumbnail")?.Attribute("url")?.Value ?? string.Empty;
                DateTime videoPublished = DateTime.Parse(videoEntry.Element(atomNs + "published")?.Value ?? DateTime.MinValue.ToString());
                string videoAuthor = videoEntry.Element(atomNs + "author")?.Element(atomNs + "name")?.Value ?? string.Empty;

                logger.LogInformation($"Retrieved post: {videoTitle} by {videoAuthor}, published on {videoPublished}");

                return new Post(videoTitle, videoThumbnail, string.Empty, videoLink, channelTitle, videoPublished, videoAuthor);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error retrieving RSS feed from URL: {xmlUrl}");
                return null;
            }
        }
    }
}