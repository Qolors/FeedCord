using System.Net;
using System.Xml.Linq;
using HtmlAgilityPack;
using FeedCord.src.Common;
using CodeHollow.FeedReader.Feeds;
using CodeHollow.FeedReader;

namespace FeedCord.src.Services.Helpers
{
    public static class PostBuilder
    {
        private static string ParseDescription(string source)
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;

            string decoded = WebUtility.HtmlDecode(source);

            // 2. Use HtmlAgilityPack to parse
            var doc = new HtmlDocument();
            doc.LoadHtml(decoded);

            // 3. Extract the "pure text"
            return doc.DocumentNode.InnerText;
        }

        private static string TryGetAuthor(FeedItem post)
        {
            try
            {
                if (!string.IsNullOrEmpty(post.Author))
                {
                    return post.Author;
                }
                if (post.SpecificItem != null)
                {
                    if (!string.IsNullOrEmpty((post.SpecificItem as MediaRssFeedItem)?.DC.Creator))
                    {
                        return (post.SpecificItem as MediaRssFeedItem).DC.Creator;
                    }
                    if (!string.IsNullOrEmpty(((Rss20FeedItem)post.SpecificItem)?.DC.Creator))
                    {
                        return ((Rss20FeedItem)post.SpecificItem).DC.Creator;
                    }
                    if (!string.IsNullOrEmpty((post.SpecificItem as MediaRssFeedItem)?.Source.Value))
                    {
                        return (post.SpecificItem as MediaRssFeedItem).Source.Value;
                    }
                }
            }
            catch
            {
                return string.Empty;
            }

            return string.Empty;
        }

        public static async Task<Post?> TryBuildPost(FeedItem post, Feed feed, int trim, string imageUrl)
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
                imageLink = imageUrl;
                description = ParseDescription(post.Description);
                link = atomItem.Links.FirstOrDefault()?.Href ?? string.Empty;
                subtitle = feed.Title;
                pubDate = DateTime.TryParse(atomItem.PublishedDate.ToString(), out var tempDate) ? tempDate : default;
                author = TryGetAuthor(post);
            }
            else
            {
                title = post.Title;
                imageLink = imageUrl;
                description = ParseDescription(post.Description);
                link = post.Link ?? string.Empty;
                subtitle = feed.Title;
                pubDate = DateTime.TryParse(post.PublishingDate.ToString(), out var tempDate) ? tempDate : default;
                author = TryGetAuthor(post);
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
