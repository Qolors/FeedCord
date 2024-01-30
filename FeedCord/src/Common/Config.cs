namespace FeedCord.src.Common
{
    public class Config
    {
        public string Id { get; set; }
        public string[] RssUrls { get; set; }
        public string[] YoutubeUrls { get; set; }
        public string DiscordWebhookUrl { get; set; }
        public string? Username { get; set; }
        public string? AvatarUrl { get; set; }
        public string? AuthorIcon { get; set; }
        public string? AuthorName { get; set; }
        public string? AuthorUrl { get; set; }
        public string? FallbackImage { get; set; }
        public string? FooterImage { get; set; }
        public int Color { get; set; }
        public int RssCheckIntervalMinutes { get; set; }
        public bool EnableAutoRemove { get; set; }
        public int DescriptionLimit { get; set; }
        public bool Forum { get; set; }
    }
}
