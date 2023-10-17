using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FeedCord.src.RssReader
{
    internal class FeedProcessor : IFeedProcessor
    {
        private readonly Config config;
        private readonly HttpClient httpClient;
        private readonly IRssProcessorService rssProcessorService;
        private readonly ILogger<FeedProcessor> logger;
        private readonly Dictionary<string, DateTime> rssFeedData = new();
        private readonly Dictionary<string, DateTime> youtubeFeedData = new();

        private FeedProcessor(
            Config config,
            IHttpClientFactory httpClientFactory,
            IRssProcessorService rssProcessorService,
            ILogger<FeedProcessor> logger)
        {
            this.config = config;
            this.httpClient = httpClientFactory.CreateClient("Default");
            this.rssProcessorService = rssProcessorService;
            this.logger = logger;
        }
        //STATIC FACTORY METHOD TO FIRST INITIALIZE URLS
        public static async Task<FeedProcessor> CreateAsync(
        Config config,
        IHttpClientFactory httpClientFactory,
        IRssProcessorService rssProcessorService,
        ILogger<FeedProcessor> logger)
        {
            var processor = new FeedProcessor(config, httpClientFactory, rssProcessorService, logger);
            await processor.InitializeUrlsAsync();
            return processor;
        }

        public async Task<List<Post>> CheckForNewPostsAsync()
        {
            ConcurrentBag<Post> newPosts = new();

            var rsstasks = rssFeedData.Select(rssFeed => CheckAndAddNewPostAsync(rssFeed, newPosts, false)).ToList();
            var youtubetasks = youtubeFeedData.Select(youtubeFeed => CheckAndAddNewPostAsync(youtubeFeed, newPosts, true)).ToList();

            var tasks = rsstasks.Concat(youtubetasks).ToList();

            await Task.WhenAll(tasks);

            return newPosts.ToList();
        }

        private async Task InitializeUrlsAsync()
        {
            int totalUrls = config.Urls.Length + config.YoutubeUrls.Length;
            int rssCount = await GetSuccessCount(config.Urls, false);
            int youtubeCount = await GetSuccessCount(config.YoutubeUrls, true);

            int successCount = rssCount + youtubeCount;

            logger.LogInformation("Set initial datetime for {UrlCount} out of {TotalUrls} on first run", successCount, totalUrls);
        }

        private async Task<int> GetSuccessCount(string[] urls, bool isYoutube)
        {
            int successCount = 0;
            foreach (var url in urls)
            {
                if (rssFeedData.ContainsKey(url))
                    continue;

                var isSuccess = await TestUrlAsync(url);

                if (isSuccess)
                {
                    if (isYoutube)
                    {
                        youtubeFeedData[url] = DateTime.Now;
                    }
                    else
                    {
                        rssFeedData[url] = DateTime.Now;
                    }
                    successCount++;
                    logger.LogInformation("Successfully initialized URL: {Url}", url);
                }
                else
                {
                    logger.LogWarning("Failed to initialize URL: {Url}", url);
                }
            }

            return successCount;
        }

        private async Task<bool> TestUrlAsync(string url)
        {
            try
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        private async Task CheckAndAddNewPostAsync(KeyValuePair<string, DateTime> rssFeed, ConcurrentBag<Post> newPosts, bool isYoutube)
        {

            var post = await CheckFeedForUpdatesAsync(rssFeed.Key, isYoutube);

            if (post != null && post.PublishDate > rssFeed.Value)
            {
                if (isYoutube)
                {
                    youtubeFeedData[rssFeed.Key] = post.PublishDate;
                }
                else
                {
                    rssFeedData[rssFeed.Key] = post.PublishDate;
                }

                newPosts.Add(post);
                logger.LogInformation("Found new post for Url: {RssFeedKey} at {CurrentTime}", rssFeed.Key, DateTime.Now);
            }
        }


        private async Task<Post?> CheckFeedForUpdatesAsync(string url, bool isYoutube)
        {
            try
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string xmlContent = await response.Content.ReadAsStringAsync();
                return isYoutube ? 
                    await rssProcessorService.ParseYoutubeFeedAsync(xmlContent) :
                    await rssProcessorService.ParseRssFeedAsync(xmlContent);
                    
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning("Failed to fetch or process the RSS feed from {Url}: Response Ended Prematurely - Skipping Url", url);
                return null;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "An unexpected error occurred while checking the RSS feed from {Url}", url);
                return null;
            }
        }

    }
}
