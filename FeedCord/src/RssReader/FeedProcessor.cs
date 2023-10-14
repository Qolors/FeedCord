using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;

namespace FeedCord.src.RssReader
{
    internal class FeedProcessor : IFeedProcessor
    {
        private readonly Config config;
        private readonly HttpClient httpClient;
        private readonly IRssProcessorService rssProcessorService;
        private Dictionary<string, DateTime> rssFeedData;
        private bool isInitialized = false;

        public FeedProcessor(Config config, IHttpClientFactory httpClientFactory, IRssProcessorService rssProcessorService)
        {
            this.config = config;
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
                    Console.WriteLine($"Updated - {rssFeed.Key}");
                    newPosts.Add(post);
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
        }
        private async Task<Post> CheckFeedForUpdatesAsync(string url)
        {
            Console.WriteLine($"Checking feed for {url}");
            var response = await httpClient.GetAsync(url);
            string xmlContent = await response.Content.ReadAsStringAsync();
            return await rssProcessorService.ParseRssFeedAsync(xmlContent);
        }
    }
}
