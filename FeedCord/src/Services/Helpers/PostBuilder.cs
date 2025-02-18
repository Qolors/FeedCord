using System.Net;
using System.Xml.Linq;
using HtmlAgilityPack;
using FeedCord.Common;
using CodeHollow.FeedReader.Feeds;
using CodeHollow.FeedReader;
using System.Text.RegularExpressions;

namespace FeedCord.Services.Helpers
{
    public static partial class PostBuilder
    {
        private static string DecodeContent(string source)
        {
            
            if (string.IsNullOrEmpty(source))
                return string.Empty;

            var decoded = WebUtility.HtmlDecode(source);
            
            //WebUtility Decode apparently parses this inconsistently, so manually converting it here
            decoded = decoded.Replace("&apos;", "'");
            
            //Respects line break encoding
            decoded = LineBreakRegex().Replace(decoded, Environment.NewLine);
            
            var doc = new HtmlDocument();
            doc.LoadHtml(decoded);
            
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
                        return (post.SpecificItem as MediaRssFeedItem)!.DC.Creator;
                    }
                    if (!string.IsNullOrEmpty(((Rss20FeedItem)post.SpecificItem)?.DC.Creator))
                    {
                        return (post.SpecificItem as Rss20FeedItem)!.DC.Creator;
                    }
                    if (!string.IsNullOrEmpty((post.SpecificItem as MediaRssFeedItem)?.Source.Value))
                    {
                        return (post.SpecificItem as MediaRssFeedItem)!.Source.Value;
                    }
                }
            }
            catch
            {
                return string.Empty;
            }

            return string.Empty;
        }

        public static Post TryBuildPost(
            FeedItem post,
            Feed feed,
            int trim,
            string imageUrl)
        {
            if (feed.Link.Contains("reddit.com"))
            {
                return TryBuildRedditPost(post, feed, trim, imageUrl);
            }

            string title;
            string imageLink;
            string description;
            string link;
            string subtitle;
            DateTime pubDate;

            if (post.SpecificItem is AtomFeedItem atomItem)
            {
                title = atomItem.Title;
                imageLink = imageUrl;
                description = DecodeContent(atomItem.Content);
                link = atomItem.Links.FirstOrDefault()?.Href ?? string.Empty;
                subtitle = feed.Title;
                pubDate = DateTime.TryParse(atomItem.PublishedDate?.ToString(), out var tempDate) 
                    ? tempDate 
                    : DateTime.TryParse(atomItem.UpdatedDate?.ToString(), out tempDate) 
                        ? tempDate 
                        : default;
            }
            else
            {
                title = post.Title;
                imageLink = imageUrl;
                description = DecodeContent(post.Description);
                link = post.Link ?? string.Empty;
                subtitle = feed.Title;
                pubDate = DateTime.TryParse(post.PublishingDate.ToString(), out var tempDate) ? tempDate : default;
            }

            var author = TryGetAuthor(post);
            
            var decTitle = DecodeContent(title);
            var decSubtitle = DecodeContent(subtitle);
            var decAuthor = DecodeContent(author);

            if (trim == 0) 
                return new Post(title, imageLink, description, link, subtitle, pubDate, author);
            
            if (description.Length > trim)
            {
                description = string.Concat(description.AsSpan(0, trim), "...");
            }
            
            return new Post(
                decTitle,
                imageLink, 
                description, 
                link, 
                decSubtitle, 
                pubDate, 
                decAuthor);
        }

        private static Post TryBuildRedditPost(
            FeedItem post,
            Feed feed,
            int trim,
            string fallbackImageUrl)
        {
            var title = post.Title ?? string.Empty;
            var imageLink = fallbackImageUrl;
            var link = post.Link ?? string.Empty;
            var description = DecodeContent(post.Description ?? string.Empty);
            var subtitle = feed.Title;
            var author = string.Empty;
            var pubDate = DateTime.MinValue;
            
            if (post.SpecificItem is AtomFeedItem { Element: not null } atomItem)
            {
                title = atomItem.Title;
                author = TryGetRedditAuthor(atomItem);

                XNamespace mediaNs = "http://search.yahoo.com/mrss/";
                var mediaThumbnail = atomItem.Element.Element(mediaNs + "thumbnail");
                if (mediaThumbnail != null)
                {
                    var thumbUrl = mediaThumbnail.Attribute("url")?.Value;
                    if (!string.IsNullOrEmpty(thumbUrl))
                        imageLink = thumbUrl;
                }
                else
                {
                    var contentElement = atomItem.Element.Element(atomItem.Element.Name.Namespace + "content");
                    if (contentElement != null)
                    {
                        var contentHtml = contentElement.Value;
                        var potentialImg = ParseFirstImageFromHtml(contentHtml);
                        if (!string.IsNullOrEmpty(potentialImg))
                            imageLink = potentialImg;
                    }
                }

                var contentEl = atomItem.Element.Element(atomItem.Element.Name.Namespace + "content");
                if (contentEl != null)
                {
                    description = DecodeContent(contentEl.Value);
                }
                else
                {
                    if (post.Description != null) 
                        description = DecodeContent(post.Description);
                }
                
                var altLink = atomItem.Links
                    .FirstOrDefault(l => l.Relation == "alternate")
                    ?? atomItem.Links.FirstOrDefault();

                if (altLink != null && !string.IsNullOrWhiteSpace(altLink.Href))
                    link = altLink.Href;
                
                var parsedDate = atomItem.PublishedDate;

                pubDate = parsedDate ?? DateTime.Now;
            }

            if (!string.IsNullOrEmpty(author) && author.StartsWith("/u/", StringComparison.OrdinalIgnoreCase))
            {
                var pattern = $@"\s*submitted by\s*{Regex.Escape(author)}\s*\[link\]\s*\[comments\]\s*";
                description = Regex.Replace(description, pattern, string.Empty, RegexOptions.IgnoreCase);
            }
            
            if (trim > 0 && description.Length > trim)
            {
                description = description[..trim] + "...";
            }
            
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
            var match = HtmlRegex().Match(html);
            
            return match.Success ? match.Groups["src"].Value : string.Empty;
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

        [GeneratedRegex("<img[^>]+src\\s*=\\s*['\"](?<src>[^'\"]+)['\"]", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex HtmlRegex();
        [GeneratedRegex("<br\\s*/?>", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex LineBreakRegex();
    }


}
