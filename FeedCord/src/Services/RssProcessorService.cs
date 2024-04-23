using CodeHollow.FeedReader;
using CodeHollow.FeedReader.Feeds;
using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;
using FeedCord.src.Helpers;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

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

        public async Task<List<Post?>> ParseRssFeedAsync(string xmlContent, int trim)
        {
            string xmlContenter = xmlContent.Replace("<!doctype", "<!DOCTYPE");

            try
            {
                var feed = FeedReader.ReadFromString(xmlContenter);

                var latestPost = feed.Items.FirstOrDefault();

                if (latestPost is null)
                    return null;

                var feedItems = feed.Items.ToList();

                List<Post?> posts = new();

                foreach (var post in feedItems)
                {
                    var builtPost = await TryBuildPost(post, feed, trim);

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

        private async Task<Post?> TryBuildPost(FeedItem post, Feed feed, int trim)
        {
            string title;
            string imageLink;
            string description;
            string link;
            string subtitle;
            DateTime pubDate;
            string author;

            if (post.SpecificItem is AtomFeedItem)
            {
                XNamespace mediaNs = "http://search.yahoo.com/mrss/";

                var atomItem = post.SpecificItem as AtomFeedItem;

                var mediaThumbnail = atomItem.Element.Element(mediaNs + "thumbnail");

                title = atomItem.Title;
                imageLink = mediaThumbnail?.Attribute("url")?.Value ?? feed.ImageUrl;
                description = StringHelper.StripTags(atomItem.Content ?? string.Empty);
                link = atomItem.Links.FirstOrDefault()?.Href ?? string.Empty;
                subtitle = feed.Title;
                pubDate = DateTime.TryParse(atomItem.PublishedDate.ToString(), out var tempDate) ? tempDate : default;
                author = !string.IsNullOrEmpty(post.Author) ? post.Author :
                         !string.IsNullOrEmpty((post.SpecificItem as MediaRssFeedItem)?.DC.Creator) ? (post.SpecificItem as MediaRssFeedItem).DC.Creator :
                         !string.IsNullOrEmpty((post.SpecificItem as MediaRssFeedItem)?.Source.Value) ? (post.SpecificItem as MediaRssFeedItem).Source.Value :
                         "";
            }
            else
            {
                title = post.Title;
                imageLink = await openGraphService.ExtractImageUrl(post.Link) ?? feed.ImageUrl;
                description = StringHelper.StripTags(post.Description ?? string.Empty);
                link = post.Link ?? string.Empty;
                subtitle = feed.Title;
                pubDate = DateTime.TryParse(post.PublishingDate.ToString(), out var tempDate) ? tempDate : default;
                author = !string.IsNullOrEmpty(post.Author) ? post.Author :
                         !string.IsNullOrEmpty((post.SpecificItem as MediaRssFeedItem)?.DC.Creator) ? (post.SpecificItem as MediaRssFeedItem).DC.Creator :
                         !string.IsNullOrEmpty((post.SpecificItem as MediaRssFeedItem)?.Source.Value) ? (post.SpecificItem as MediaRssFeedItem).Source.Value :
                         "";
            }

            if (trim != 0)
            {
                if (description.Length > trim)
                {
                    description = description.Substring(0, trim) + "...";
                }
            }

            return new Post(title, imageLink, description, link, subtitle, pubDate, author);
        }
    }
}
