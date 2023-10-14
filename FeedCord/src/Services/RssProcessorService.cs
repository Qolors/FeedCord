using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace FeedCord.src.Services
{
    internal class RssProcessorService : IRssProcessorService
    {
        public async Task<Post> ParseRssFeedAsync(string xmlContent)
        {
            var xDoc = XDocument.Parse(xmlContent);

            XNamespace media = xDoc.Root.GetNamespaceOfPrefix("media");

            string subtitle = xDoc.Descendants("title").FirstOrDefault().Value;

            var latestPost = xDoc.Descendants("item").FirstOrDefault();

            if (latestPost == null)
            {
                throw new InvalidOperationException("No items found in the RSS feed.");
            }

            string rawDescription = latestPost?.Element("description")?.Value ?? string.Empty;
            string rawLink = string.Empty;
            if (media != null)
            {
                rawLink = latestPost?.Element(media + "thumbnail")?.Attribute("url")?.Value ?? string.Empty;
            }
            else
            {
                rawLink = latestPost?.Element("description")?.Value ?? string.Empty;
            }
            string rawTitle = latestPost?.Element("title")?.Value ?? string.Empty;

            string description = StripTags(rawDescription);
            string title = StripTags(rawTitle);

            string link = !string.IsNullOrWhiteSpace(rawLink)
                ? rawLink
                : ExtractUrls(rawDescription).FirstOrDefault() ?? string.Empty;

            Post post = new Post(
                title,
                link,  // Assuming media:thumbnail has a 'url' attribute
                description,
                latestPost?.Element("link")?.Value ?? string.Empty,
                subtitle,
                DateTime.TryParse(latestPost?.Element("pubDate")?.Value, out var pubDate) ? pubDate : default
            );

            return await Task.FromResult(post);
        }

        public string StripTags(string source)
        {
            return Regex.Replace(source, "<.*?>", string.Empty);
        }

        public IEnumerable<string> ExtractUrls(string source)
        {
            var matches = Regex.Matches(source, @"https?://\S+");

            foreach (Match match in matches)
            {
                yield return match.Value;
            }
        }
    }
}
