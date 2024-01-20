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
        private readonly Dictionary<string, int> rssFeedErrorTracker = new();

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

            var rsstasks = rssFeedData.Select(rssFeed => CheckAndAddNewPostAsync(rssFeed, newPosts, false, config.DescriptionLimit)).ToList();
            var youtubetasks = youtubeFeedData.Select(youtubeFeed => CheckAndAddNewPostAsync(youtubeFeed, newPosts, true, config.DescriptionLimit)).ToList();

            var tasks = rsstasks.Concat(youtubetasks).ToList();

            await Task.WhenAll(tasks);

            return newPosts.ToList();
        }

        private async Task InitializeUrlsAsync()
        {
            int totalUrls = 0;
            int rssCount = await GetSuccessCount(config.Urls, false);
            int youtubeCount = await GetSuccessCount(config.YoutubeUrls, true);
            int successCount = 0;

            if (config.YoutubeUrls is null || config.YoutubeUrls.Length == 1 && string.IsNullOrEmpty(config.YoutubeUrls[0]))
            {
                totalUrls = config.Urls.Length;
            }
            else
            {
                totalUrls = config.Urls.Length + config.YoutubeUrls.Length;
            }

            successCount = rssCount + youtubeCount;

            logger.LogInformation("Tested successfully for {UrlCount} out of {TotalUrls} Urls in Configuration File", successCount, totalUrls);
        }

        private async Task<int> GetSuccessCount(string[] urls, bool isYoutube)
        {
            int successCount = 0;

            if (urls.Length == 0 || urls.Length == 1 && string.IsNullOrEmpty(urls[0]))
            {
                string type = isYoutube ? "Youtube" : "RSS";
                logger.LogInformation("No URLs in {type} feed, skipping...", type);
                return successCount;
            }

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

                    rssFeedErrorTracker[url] = 0;
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

        private async Task CheckAndAddNewPostAsync(KeyValuePair<string, DateTime> rssFeed, ConcurrentBag<Post> newPosts, bool isYoutube, int trim)
        {
            logger.LogInformation("Checking if any new posts for {RssFeedKey}...", rssFeed.Key);

            var post = await CheckFeedForUpdatesAsync(rssFeed.Key, isYoutube, trim);

            if (post is null)
            {
                rssFeedErrorTracker[rssFeed.Key]++;
                logger.LogWarning("Failed to fetch or process the RSS feed from {Url}.. Error Count: {ErrorCount}", rssFeed.Key, rssFeedErrorTracker[rssFeed.Key]);

                if (rssFeedErrorTracker[rssFeed.Key] >= 3)
                {
                    if (config.EnableAutoRemove)
                    {
                        logger.LogWarning("Removing Url: {Url} from the list of RSS feeds due to too many errors", rssFeed.Key);

                        if (youtubeFeedData.ContainsKey(rssFeed.Key))
                        {
                            youtubeFeedData.Remove(rssFeed.Key);
                        }
                        else if (rssFeedData.ContainsKey(rssFeed.Key))
                        {
                            rssFeedData.Remove(rssFeed.Key);
                        }

                        rssFeedErrorTracker.Remove(rssFeed.Key);
                    }
                    else
                    {
                        logger.LogWarning("Recommend enabling Auto Remove or manually removing the url from the config file");
                    }

                    return;
                }

                return;
            }

            if (post.PublishDate <= rssFeed.Value)
            {
                return;
            }

            if (isYoutube)
            {
                youtubeFeedData[rssFeed.Key] = post.PublishDate;
            }
            else
            {
                rssFeedData[rssFeed.Key] = post.PublishDate;
            }

            newPosts.Add(post);
            logger.LogInformation("Found new post for Url: {RssFeedKey}", rssFeed.Key);
        }



        private async Task<Post?> CheckFeedForUpdatesAsync(string url, bool isYoutube, int trim)
        {
            try
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string xmlContent = isYoutube ?
                    await response.Content.ReadAsStringAsync() :
                    url;
                return isYoutube ? 
                    await rssProcessorService.ParseYoutubeFeedAsync(xmlContent) :
                    await rssProcessorService.ParseRssFeedAsync(xmlContent, trim);
                    
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
