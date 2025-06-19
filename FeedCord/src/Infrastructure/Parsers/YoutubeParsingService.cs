using System.Globalization;
using FeedCord.Common;
using FeedCord.Infrastructure.Http;
using FeedCord.Services.Interfaces;
using System.Xml.Linq;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace FeedCord.Infrastructure.Parsers
{
    public class YoutubeParsingService : IYoutubeParsingService
    {
        private readonly ICustomHttpClient _httpClient;
        private readonly ILogger<YoutubeParsingService> _logger;
        public YoutubeParsingService(ICustomHttpClient httpClient, ILogger<YoutubeParsingService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Post?> GetXmlUrlAndFeed(string xml)
        {
            if (xml.StartsWith("https") && xml.Contains("xml"))
            {
                return await GetRecentPost(xml);
            }
                

            var doc = new HtmlDocument();
            doc.LoadHtml(xml);

            var node = doc.DocumentNode.SelectSingleNode("//link[@rel='alternate' and @type='application/rss+xml']");

            if (node != null)
            {
                var hrefValue = node.GetAttributeValue("href", "");
                return await GetRecentPost(hrefValue);
            }

            _logger.LogWarning("No RSS feed link found in the provided XML.");
            return null;
        }

        private async Task<Post?> GetRecentPost(string xmlUrl)
        {
            if (string.IsNullOrEmpty(xmlUrl))
            {
                return null;
            }

            try
            {
                var response = await _httpClient.GetAsyncWithFallback(xmlUrl);

                if (response is null) return null;
                
                response.EnsureSuccessStatusCode();

                var xmlContent = await response.Content.ReadAsStringAsync();

                var xdoc = XDocument.Parse(xmlContent);
                if (xdoc.Root == null) return null;

                XNamespace atomNs = "http://www.w3.org/2005/Atom";
                XNamespace mediaNs = "http://search.yahoo.com/mrss/";

                var channelTitle = xdoc.Root.Element(atomNs + "title")?.Value ?? string.Empty;
                var videoEntry = xdoc.Root.Element(atomNs + "entry");

                if (videoEntry is null)
                {
                    return null;
                }

                var videoTitle = videoEntry.Element(atomNs + "title")?.Value ?? string.Empty;
                var videoLink = videoEntry.Element(atomNs + "link")?.Attribute("href")?.Value ?? string.Empty;
                var videoThumbnail = videoEntry.Element(mediaNs + "group")?.Element(mediaNs + "thumbnail")?.Attribute("url")?.Value ?? string.Empty;
                var videoPublished = DateTime.Parse(videoEntry.Element(atomNs + "published")?.Value ?? DateTime.MinValue.ToString(CultureInfo.CurrentCulture));
                var videoAuthor = videoEntry.Element(atomNs + "author")?.Element(atomNs + "name")?.Value ?? string.Empty;
                

                return new Post(videoTitle, videoThumbnail, string.Empty, videoLink, channelTitle, videoPublished, videoAuthor);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving RSS feed from URL: {Ex}", ex);
                return null;
            }
        }
    }
}