

namespace FeedCord.src.Common
{
    internal class Config
    {
        private readonly string[] urls;
        private readonly string webhook;
        private int rssCheckIntervalMinutes;
        public string[] Urls => urls;
        public string Webhook => webhook;
        public int RssCheckIntervalMinutes => rssCheckIntervalMinutes;
        public Config(string[] urls, string webhook, int rssCheckIntervalMinutes)
        {
            this.urls = urls;
            this.webhook = webhook;
            this.rssCheckIntervalMinutes = rssCheckIntervalMinutes;
        }
    }
}
