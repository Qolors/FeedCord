using System.Xml.Linq;
using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace FeedCord.src.Services
{
    public class YoutubeParsingService : IYoutubeParsingService
    {
        private HttpClient httpClient;
        private ILogger<YoutubeParsingService> logger;
        public YoutubeParsingService(IHttpClientFactory httpClientFactory, ILogger<YoutubeParsingService> logger)
        {
            this.httpClient = httpClientFactory.CreateClient("Default");
            this.logger = logger;
        }

        public async Task<Post?> GetXmlUrlAndFeed(string xml)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(xml);

            var node = doc.DocumentNode.SelectSingleNode("//link[@rel='alternate' and @type='application/rss+xml']");

            if (node != null)
            {
                var hrefValue = node.GetAttributeValue("href", "");
                return await GetRecentPost(hrefValue);
            }

            return null;
        }

        private async Task<Post?> GetRecentPost(string xmlUrl)
        {
            if (string.IsNullOrEmpty(xmlUrl))
                return null;

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(xmlUrl);
                response.EnsureSuccessStatusCode();

                string xmlContent = await response.Content.ReadAsStringAsync();

                XDocument xdoc = XDocument.Parse(xmlContent);
                XNamespace atomNs = "http://www.w3.org/2005/Atom";
                XNamespace mediaNs = "http://search.yahoo.com/mrss/";

                var channelTitle = xdoc.Root.Element(atomNs + "title").Value ?? string.Empty;

                var videoEntry = xdoc.Root.Element(atomNs + "entry");

                if (videoEntry is null)
                    return null;

                var videoTitle = videoEntry.Element(atomNs + "title").Value ?? string.Empty;
                var videoLink = videoEntry.Element(atomNs + "link").Attribute("href").Value ?? string.Empty;
                var videoThumbnail = videoEntry.Element(mediaNs + "group").Element(mediaNs + "thumbnail").Attribute("url").Value ?? string.Empty;
                DateTime videoPublished = DateTime.Parse(videoEntry.Element(atomNs + "published").Value);

                return new Post(videoTitle, videoThumbnail, string.Empty, videoLink, channelTitle, videoPublished);
            }
            catch(Exception ex)
            {
                return null;
            }
        }
    }
}