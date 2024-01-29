namespace FeedCord.src.Common
{
    public class Config
    {
        public string[] RssUrls { get; }
        public string[] YoutubeUrls { get; }
        public string DiscordWebhookUrl { get; }
        public string Username { get; }
        public string AvatarUrl { get; }
        public string AuthorIcon { get; }
        public string AuthorName { get; }
        public string AuthorUrl { get; }
        public string FallbackImage { get; }
        public string FooterImage { get; }
        public int Color { get; }
        public int RssCheckIntervalMinutes { get; }
        public bool EnableAutoRemove { get; }
        public int DescriptionLimit { get; }
        public bool Forum { get; }

        public Config() { }
    }
}
