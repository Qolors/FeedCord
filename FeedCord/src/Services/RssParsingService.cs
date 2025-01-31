using Microsoft.Extensions.Logging;
using CodeHollow.FeedReader;
using FeedCord.src.Common;
using FeedCord.src.Services.Helpers;
using FeedCord.src.Services.Interfaces;

namespace FeedCord.src.Services
{
    public class RssParsingService : IRssParsingService
    {
        private readonly ILogger<RssParsingService> logger;
        private readonly IYoutubeParsingService youtubeParsingService;
        private readonly IImageParserService imageParserService;

        public RssParsingService(
            ILogger<RssParsingService> logger,  
            IYoutubeParsingService youtubeParsingService,
            IImageParserService imageParserService)
        {
            this.logger = logger;
            this.youtubeParsingService = youtubeParsingService;
            this.imageParserService = imageParserService;
        }

        public async Task<List<Post?>> ParseRssFeedAsync(string xmlContent, int trim)
        {
            string xmlContenter = xmlContent.Replace("<!doctype", "<!DOCTYPE");

            try
            {
                var feed = FeedReader.ReadFromString(xmlContenter);

                var latestPost = feed.Items.FirstOrDefault();

                if (latestPost is null)
                    return new List<Post?>();

                var feedItems = feed.Items.ToList();

                List<Post?> posts = new();

                foreach (var post in feedItems)
                {
                    string imageLink = await imageParserService
                        .TryExtractImageLink(post.Link) ?? feed.ImageUrl;

                    var builtPost = await PostBuilder.TryBuildPost(post, feed, trim, imageLink);

                    if (builtPost is not null)
                    {
                        posts.Add(builtPost);
                    }
                }

                return posts;

            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "An unexpected error occurred while parsing the RSS feed");
                return null;
            }
        }

        public async Task<Post?> ParseYoutubeFeedAsync(string channelUrl)
        {
            return await youtubeParsingService.GetXmlUrlAndFeed(channelUrl);
        }
    }
}
