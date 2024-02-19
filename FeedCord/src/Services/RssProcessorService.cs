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

        public async Task<Post?> ParseRssFeedAsync(string xmlContent, int trim)
        {
            string xmlContenter = xmlContent.Replace("<!doctype", "<!DOCTYPE");

            try
            {
                var feed = FeedReader.ReadFromString(xmlContenter);

                var latestPost = feed.Items.FirstOrDefault();

                if (latestPost == null)
                    return null;

                string title;
                string imageLink;
                string description;
                string link;
                string subtitle;
                DateTime pubDate;

                if (latestPost.SpecificItem is AtomFeedItem)
                {
                    XNamespace mediaNs = "http://search.yahoo.com/mrss/";

                    var sortedItems = feed.Items.OrderByDescending(item => item.PublishingDate).ToList();

                    // The first item in the sorted list will be the latest post
                    latestPost = sortedItems.FirstOrDefault(); 

                    var atomItem = latestPost.SpecificItem as AtomFeedItem;

                    var mediaThumbnail = atomItem.Element.Element(mediaNs + "thumbnail");

                    title = atomItem.Title;
                    imageLink = mediaThumbnail?.Attribute("url")?.Value ?? feed.ImageUrl;
                    description = StringHelper.StripTags(atomItem.Content ?? string.Empty);
                    link = atomItem.Links.FirstOrDefault()?.Href ?? string.Empty;
                    subtitle = feed.Title;
                    pubDate = DateTime.TryParse(atomItem.PublishedDate.ToString(), out var tempDate) ? tempDate : default;
                }
                else
                {
                    title = latestPost.Title;
                    imageLink = await openGraphService.ExtractImageUrl(latestPost.Link) ?? feed.ImageUrl;
                    description = StringHelper.StripTags(latestPost.Description ?? string.Empty);
                    link = latestPost.Link ?? string.Empty;
                    subtitle = feed.Title;
                    pubDate = DateTime.TryParse(latestPost.PublishingDate.ToString(), out var tempDate) ? tempDate : default;
                }

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
