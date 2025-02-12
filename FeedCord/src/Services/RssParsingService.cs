using Microsoft.Extensions.Logging;
using CodeHollow.FeedReader;
using FeedCord.Common;
using FeedCord.Services.Helpers;
using FeedCord.Services.Interfaces;

namespace FeedCord.Services
{
    public class RssParsingService : IRssParsingService
    {
        private readonly ILogger<RssParsingService> _logger;
        private readonly IYoutubeParsingService _youtubeParsingService;
        private readonly IImageParserService _imageParserService;

        public RssParsingService(
            ILogger<RssParsingService> logger,
            IYoutubeParsingService youtubeParsingService,
            IImageParserService imageParserService)
        {
            _logger = logger;
            _youtubeParsingService = youtubeParsingService;
            _imageParserService = imageParserService;
        }

        public async Task<List<Post?>> ParseRssFeedAsync(string xmlContent, int trim)
        {
            var xmlContenter = xmlContent.Replace("<!doctype", "<!DOCTYPE");

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
                    var rawXml = GetRawXmlForItem(post);

                    var imageLink = await _imageParserService
                        .TryExtractImageLink(post.Link, rawXml) 
                                    ?? feed.ImageUrl;

                    var builtPost = PostBuilder.TryBuildPost(post, feed, trim, imageLink);

                    posts.Add(builtPost);
                }

                return posts;

            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "An unexpected error occurred while parsing the RSS feed");
                return new List<Post?>();
            }
        }

        public async Task<Post?> ParseYoutubeFeedAsync(string channelUrl)
        {
            return await _youtubeParsingService.GetXmlUrlAndFeed(channelUrl);
        }

        private string GetRawXmlForItem(FeedItem feedItem)
        {
            if (feedItem.SpecificItem is CodeHollow.FeedReader.Feeds.Rss20FeedItem rssItem)
            {
                return rssItem.Element?.ToString() ?? "";
            }
            else if (feedItem.SpecificItem is CodeHollow.FeedReader.Feeds.AtomFeedItem atomItem)
            {
                return atomItem.Element?.ToString() ?? "";
            }

            return "";
        }

    }
}
