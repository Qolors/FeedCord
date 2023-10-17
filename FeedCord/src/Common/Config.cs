namespace FeedCord.src.Common
{
    internal class Config
    {
        public string[] Urls { get; }
        public string[] YoutubeUrls { get; }
        public string Webhook { get; }
        public string Username { get; }
        public string AvatarUrl { get; }
        public string AuthorIcon { get; }
        public string AuthorName { get; }
        public string AuthorUrl { get; }
        public string FallbackImage { get; }
        public string FooterImage { get; }
        public int Color { get; }
        public int RssCheckIntervalMinutes { get; }

        public Config(
            string[] urls,
            string[] youtubeurls,
            string webhook,
            string username,
            string avatarUrl,
            string authorIcon,
            string authorName,
            string authorUrl,
            string fallbackImage,
            string footerImage,
            int color,
            int rssCheckIntervalMinutes)
        {
            Urls = urls ?? throw new ArgumentNullException(nameof(urls));
            YoutubeUrls = youtubeurls ?? new string[0];
            Webhook = webhook ?? throw new ArgumentNullException(nameof(webhook));
            Username = username;
            AvatarUrl = avatarUrl;
            AuthorIcon = authorIcon;
            AuthorName = authorName;
            AuthorUrl = authorUrl;
            FallbackImage = fallbackImage;
            FooterImage = footerImage;
            Color = color;
            RssCheckIntervalMinutes = rssCheckIntervalMinutes;
        }
    }
}
