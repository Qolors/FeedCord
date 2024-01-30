using CodeHollow.FeedReader;
using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;
using FeedCord.src.Helpers;
using Microsoft.Extensions.Logging;

namespace FeedCord.src.Services
{
    public class RssProcessorService : IRssProcessorService
    {
        private readonly ILogger<RssProcessorService> logger;
        private readonly IYoutubeParsingService youtubeParsingService;
        private readonly IOpenGraphService openGraphService;

        public RssProcessorService(
            ILogger<RssProcessorService> logger,  
            IYoutubeParsingService youtubeParsingService,
            IOpenGraphService openGraphService)
        {
            this.logger = logger;
            this.youtubeParsingService = youtubeParsingService;
            this.openGraphService = openGraphService;
        }

        public async Task<Post?> ParseRssFeedAsync(string xmlContent, int trim)
        {
            try
            {
                var feed = await FeedReader.ReadAsync(xmlContent);

                var latestPost = feed.Items.FirstOrDefault();

                if (latestPost == null)
                    return null;

                string title = latestPost.Title;
                string imageLink = await openGraphService.ExtractImageUrl(latestPost.Link) ?? feed.ImageUrl;
                string description = StringHelper.StripTags(latestPost.Description ?? string.Empty);
                string link = latestPost.Link ?? string.Empty;
                string subtitle = feed.Title;
                DateTime pubDate = DateTime.TryParse(latestPost.PublishingDate.ToString(), out var tempDate) ? tempDate : default;

                if (trim != 0)
                {
                    if (description.Length > trim)
                    {
                        description = description.Substring(0, trim) + "...";
                    }
                }

                return new Post(title, imageLink, description, link, subtitle, pubDate);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<Post?> ParseYoutubeFeedAsync(string channelUrl)
        {
            return await youtubeParsingService.GetXmlUrlAndFeed(channelUrl);
        }
    }
}
