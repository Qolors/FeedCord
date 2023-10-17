using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;
using FeedCord.src.Helpers;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace FeedCord.src.Services
{
    internal class RssProcessorService : IRssProcessorService
    {
        private readonly ILogger<RssProcessorService> logger;
        private readonly IOpenGraphService openGraphService;
        private readonly IYoutubeParsingService youtubeParsingService;

        public RssProcessorService(
            ILogger<RssProcessorService> logger, 
            IOpenGraphService openGraphService, 
            IYoutubeParsingService youtubeParsingService)
        {
            this.logger = logger;
            this.openGraphService = openGraphService;
            this.youtubeParsingService = youtubeParsingService;
        }

        public async Task<Post?> ParseRssFeedAsync(string xmlContent)
        {
            try
            {
                var xDoc = XDocument.Parse(xmlContent);
                var subtitle = xDoc.Descendants("title").FirstOrDefault()?.Value ?? string.Empty;
                var latestPost = xDoc.Descendants("item").FirstOrDefault();

                if (latestPost == null)
                {
                    logger.LogError("No items found in the RSS feed. Is this a traditional RSS Feed?");
                    return null;
                }

                string rawDescription = latestPost.Element("description")?.Value ?? string.Empty;
                string description = StringHelper.StripTags(rawDescription);
                if (description.Length > 200)
                {
                    description = string.Concat(description.AsSpan(0, 197), "...");
                }

                string title = StringHelper.StripTags(latestPost.Element("title")?.Value ?? string.Empty);
                string imageLink = await openGraphService.ExtractImageUrl(latestPost.Element("link")?.Value ?? string.Empty);
                DateTime pubDate = DateTime.TryParse(latestPost.Element("pubDate")?.Value, out var tempDate) ? tempDate : default;

                return new Post(title, imageLink, description, latestPost.Element("link")?.Value ?? string.Empty, subtitle, pubDate);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error parsing the RSS feed.");
                return null;
            }
        }

        public async Task<Post?> ParseYoutubeFeedAsync(string channelUrl)
        {
            return await youtubeParsingService.GetXmlUrlAndFeed(channelUrl);
        }
    }
}
