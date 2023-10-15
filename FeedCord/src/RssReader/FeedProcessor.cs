using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace FeedCord.src.RssReader
{
    internal class FeedProcessor : IFeedProcessor
    {
        private readonly Config config;
        private readonly HttpClient httpClient;
        private readonly IRssProcessorService rssProcessorService;
        private readonly ILogger<FeedProcessor> logger;
        private Dictionary<string, DateTime> rssFeedData;
        private bool isInitialized = false;

        public FeedProcessor(
            Config config, 
            IHttpClientFactory httpClientFactory,
            IRssProcessorService rssProcessorService,
            ILogger<FeedProcessor> logger)
        {

            this.config = config;
            this.logger = logger;
            this.rssProcessorService = rssProcessorService;

            httpClient = httpClientFactory.CreateClient();
            rssFeedData = new();

        }
        public async Task<List<Post>> CheckForNewPostsAsync()
        {
            if (!isInitialized)
                InitializeUrls();

            List<Post> newPosts = new();

            foreach (var rssFeed in rssFeedData)
            {
                Post post = await CheckFeedForUpdatesAsync(rssFeed.Key);

                if (post.PublishDate > rssFeed.Value)
                {
                    rssFeedData[rssFeed.Key] = post.PublishDate;
                    newPosts.Add(post);
                    logger.LogInformation("[{DateTime.Now}]: Found new post for Url: {RssFeed.Key}", DateTime.Now, rssFeed.Key);
                }
                
            }

            return newPosts;
        }
        private void InitializeUrls()
        {
            for (int i = 0; i < config.Urls.Length; i++)
            {
                var url = config.Urls[i];
                
                if (!rssFeedData.ContainsKey(url))
                    rssFeedData.Add(url, DateTime.Now);
            }

            isInitialized = true;

            logger.LogInformation("[{DateTime.Now}]: Set [{DateTime.Now}] for {Config.Urls.Length} Urls on first run", DateTime.Now, DateTime.Now, config.Urls.Length);
        }
        private async Task<Post> CheckFeedForUpdatesAsync(string url)
        {
            var response = await httpClient.GetAsync(url);
            string xmlContent = await response.Content.ReadAsStringAsync();
            return await rssProcessorService.ParseRssFeedAsync(xmlContent);
        }
    }
}
