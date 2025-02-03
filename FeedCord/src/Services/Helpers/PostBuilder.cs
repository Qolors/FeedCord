using System.Net;
using System.Xml.Linq;
using HtmlAgilityPack;
using FeedCord.src.Common;
using CodeHollow.FeedReader.Feeds;
using CodeHollow.FeedReader;
using System.Text.RegularExpressions;

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

        public static async Task<Post?> TryBuildPost(
            FeedItem post, 
            Feed feed, 
            int trim, 
            string imageUrl)
        {
            if (feed.Link.Contains("reddit.com"))
            {
                return await TryBuildRedditPost(post, feed, trim, imageUrl);
            }

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

        public static async Task<Post?> TryBuildRedditPost(
            FeedItem post,
            Feed feed,
            int trim,
            string fallbackImageUrl)
        {
            // Initialize defaults
            string title = post.Title ?? string.Empty;
            string imageLink = fallbackImageUrl;
            string link = post.Link ?? string.Empty;
            string description = ParseDescription(post.Description ?? string.Empty);
            string subtitle = feed.Title;
            string author = string.Empty;
            DateTime pubDate = DateTime.MinValue;
            

            // If this is an AtomFeedItem, we can parse more details
            if (post.SpecificItem is AtomFeedItem atomItem && atomItem.Element != null)
            {

                title = atomItem.Title;

                author = TryGetRedditAuthor(atomItem);

                XNamespace mediaNs = "http://search.yahoo.com/mrss/";
                var mediaThumbnail = atomItem.Element.Element(mediaNs + "thumbnail");
                if (mediaThumbnail != null)
                {
                    // Use the thumbnail if found
                    var thumbUrl = mediaThumbnail.Attribute("url")?.Value;
                    if (!string.IsNullOrEmpty(thumbUrl))
                        imageLink = thumbUrl;
                }
                else
                {
                    // If no <media:thumbnail>, try to find <img> in <content>
                    var contentElement = atomItem.Element.Element(atomItem.Element.Name.Namespace + "content");
                    if (contentElement != null)
                    {
                        string contentHtml = contentElement.Value;
                        var potentialImg = ParseFirstImageFromHtml(contentHtml);
                        if (!string.IsNullOrEmpty(potentialImg))
                            imageLink = potentialImg;
                    }
                }

                // 3) DESCRIPTION (HTML content)
                // The <content> element often contains HTML for the post
                var contentEl = atomItem.Element.Element(atomItem.Element.Name.Namespace + "content");
                if (contentEl != null)
                {
                    description = ParseDescription(contentEl.Value);
                }
                else
                {
                    // Fallback if no <content> is found
                    description = ParseDescription(post.Description);
                }

                // 4) LINK
                // Often the AtomFeedItem.Links will contain multiple links: 
                //   - rel="self" or rel="edit"
                //   - rel="alternate" is the "main" link
                var altLink = atomItem.Links
                    .FirstOrDefault(l => l.Relation == "alternate")
                    ?? atomItem.Links.FirstOrDefault();

                if (altLink != null && !string.IsNullOrWhiteSpace(altLink.Href))
                    link = altLink.Href;

                // 5) PUBLISHED DATE
                // Atom items typically have PublishedDate and/or UpdatedDate
                var parsedDate = atomItem.PublishedDate;

                pubDate = parsedDate ?? DateTime.Now;
            }

            if (!string.IsNullOrEmpty(author) && author.StartsWith("/u/", StringComparison.OrdinalIgnoreCase))
            {
                // This regex looks for "submitted by /u/Whatever [link] [comments]" (case-insensitive).
                // Adjust to suit your exact snippet.
                string pattern = $@"\s*submitted by\s*{Regex.Escape(author)}\s*\[link\]\s*\[comments\]\s*";
                description = Regex.Replace(description, pattern, string.Empty, RegexOptions.IgnoreCase);
            }

            // Trim the description if needed
            if (trim > 0 && description.Length > trim)
            {
                description = description.Substring(0, trim) + "...";
            }

            // Finally build and return your Post object
            // (Adjust to your actual model constructor)
            return new Post(
                Title: title,
                ImageUrl: imageLink,
                Description: description,
                Link: link,
                Tag: subtitle,
                PublishDate: pubDate,
                Author: author
            );
        }

        private static string ParseFirstImageFromHtml(string html)
        {
            // You can do this with HtmlAgilityPack, Regex, or a simple approach using XDocument (after cleaning).
            // Here’s a very naive regex approach (for demonstration only):
            var match = Regex.Match(html, @"<img[^>]+src\s*=\s*['""](?<src>[^'""]+)['""]",
                RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups["src"].Value;
            }
            return string.Empty;
        }

        private static string TryGetRedditAuthor(AtomFeedItem atomItem)
        {
            try
            {
                var authorElement = atomItem.Element
                    .Element(atomItem.Element.Name.Namespace + "author");

                if (authorElement != null)
                {
                    var authorName = authorElement
                        .Element(authorElement.Name.Namespace + "name")?.Value;

                    return authorName ?? string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
            return string.Empty;
        }

    }


}
