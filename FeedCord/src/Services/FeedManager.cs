using FeedCord.src.Common;
using FeedCord.src.Helpers;
using FeedCord.src.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text;

namespace FeedCord.src.Services
{
    public class FeedManager : IFeedManager
    {
        private readonly Config config;
        private readonly ICustomHttpClient httpClient;
        private readonly ILogger<FeedManager> logger;
        private readonly IRssParsingService rssParsingService;
        private readonly Dictionary<string, ReferencePost> lastRunReference = new();
        private readonly ConcurrentDictionary<string, FeedState> _feedStates = new();

        public FeedManager(
            Config config,
            ICustomHttpClient httpClient,
            IRssParsingService rssParsingService,
            ILogger<FeedManager> logger)
        {
            this.config = config;
            this.httpClient = httpClient;
            this.lastRunReference = CsvReader.LoadReferencePosts("feed_dump.csv");
            this.rssParsingService = rssParsingService;
            this.logger = logger;
        }
        public async Task<List<Post>> CheckForNewPostsAsync()
        {
            ConcurrentBag<Post> allNewPosts = new();

            var tasks = _feedStates.Select(async (feed) => await CheckSingleFeedAsync(feed.Key, feed.Value, allNewPosts, config.DescriptionLimit));

            await Task.WhenAll(tasks);

            return allNewPosts.ToList();
        }
        public async Task InitializeUrlsAsync()
        {
            int totalUrls = 0;
            int rssCount = await GetSuccessCount(config.RssUrls, false);
            int youtubeCount = await GetSuccessCount(config.YoutubeUrls, true);
            int successCount = 0;
            string id = config.Id;

            if (config.YoutubeUrls is null || config.YoutubeUrls.Length == 1 && string.IsNullOrEmpty(config.YoutubeUrls[0]))
            {
                totalUrls = config.RssUrls.Length;
            }
            else
            {
                totalUrls = config.RssUrls.Length + config.YoutubeUrls.Length;
            }

            successCount = rssCount + youtubeCount;

            logger.LogInformation("{id}: Tested successfully for {UrlCount} out of {TotalUrls} Urls in Configuration File", id, successCount, totalUrls);
        }

        public IReadOnlyDictionary<string, FeedState> GetAllFeedData()
        {
            return _feedStates;
        }
        private async Task<int> GetSuccessCount(string[] urls, bool isYoutube)
        {
            int successCount = 0;

            if (urls.Length == 0 || urls.Length == 1 && string.IsNullOrEmpty(urls[0]))
            {
                string type = isYoutube ? "Youtube" : "RSS";
                return successCount;
            }

            foreach (var url in urls)
            {

                var isSuccess = await TestUrlAsync(url);

                if (isSuccess)
                {

                    if (lastRunReference.TryGetValue(url, out ReferencePost? value))
                    {
                        _feedStates.TryAdd(url, new FeedState
                        {
                            IsYoutube = isYoutube,
                            LastPublishDate = value.LastRunDate,
                            ErrorCount = 0
                        });

                        logger.LogInformation("Successfully initialized Existing URL: {Url}", url);

                        successCount++;

                        continue;
                    }

                    bool successfulAdd = false;

                    if (isYoutube)
                    {
                        successfulAdd = _feedStates.TryAdd(url, new FeedState
                        {
                            IsYoutube = true,
                            LastPublishDate = DateTime.Now,
                            ErrorCount = 0
                        });
                    }
                    else
                    {
                        successfulAdd = _feedStates.TryAdd(url, new FeedState
                        {
                            IsYoutube = false,
                            LastPublishDate = DateTime.Now,
                            ErrorCount = 0
                        });
                    }

                    if (successfulAdd)
                    {
                        successCount++;
                        logger.LogInformation("Successfully initialized URL: {Url}", url);
                    }

                    else
                    {
                        logger.LogWarning("Failed to initialize URL: {Url}", url);
                    }
                }
            }

            return successCount;
        }
        private async Task<bool> TestUrlAsync(string url)
        {
            try
            {
                var response = await httpClient.GetAsyncWithFallback(url);
                logger.LogInformation($"Status Code: {(int)response.StatusCode} {response.StatusCode}");

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }
        private async Task CheckSingleFeedAsync(string url, FeedState feedState, ConcurrentBag<Post> newPosts, int trim)
        {
            logger.LogInformation("Checking if any new posts for {Url}...", url);

            List<Post?> posts;
            try
            {
                posts = feedState.IsYoutube ?
                    await FetchYoutubeAsync(url, trim) :
                    await FetchRssAsync(url, trim);
            }
            catch (Exception ex)
            {
                HandleFeedError(url, feedState, ex);
                return;
            }

            var freshlyFetched = posts.Where(p => p.PublishDate > feedState.LastPublishDate).ToList();
            
            if (freshlyFetched.Any())
            {
                feedState.LastPublishDate = freshlyFetched.Max(p => p.PublishDate);
                feedState.ErrorCount = 0;

                foreach (var post in freshlyFetched)
                {
                    if (post is null)
                    {
                        logger.LogWarning("Failed to parse a post from {Url}", url);
                        continue;
                    }
                    newPosts.Add(post);
                }
            }
        }
        private async Task<List<Post?>> FetchYoutubeAsync(string url, int trim)
        {
            try
            {
                var response = await httpClient.GetAsyncWithFallback(url);

                response.EnsureSuccessStatusCode();

                string xmlContent = await GetResponseContentAsync(response);

                var post = await rssParsingService.ParseYoutubeFeedAsync(xmlContent);

                return post == null ? new List<Post?>() : new List<Post?> { post };

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

        private async Task<List<Post?>> FetchRssAsync(string url, int trim)
        {
            try
            {
                var response = await httpClient.GetAsyncWithFallback(url);

                response.EnsureSuccessStatusCode();

                string xmlContent = await GetResponseContentAsync(response);

                return await rssParsingService.ParseRssFeedAsync(xmlContent, trim);

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
        private async Task<string> GetResponseContentAsync(HttpResponseMessage response)
        {
            if (response.Content.Headers.ContentEncoding.Contains("gzip"))
            {
                using var decompressedStream = new GZipStream(await response.Content.ReadAsStreamAsync(), CompressionMode.Decompress);
                using var reader = new StreamReader(decompressedStream, Encoding.UTF8);
                return await reader.ReadToEndAsync();
            }
            else
            {
                return await response.Content.ReadAsStringAsync();
            }
        }

        private void HandleFeedError(string url, FeedState feedState, Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch or parse feed {Url}", url);
            feedState.ErrorCount++;

            if (feedState.ErrorCount >= 3 && config.EnableAutoRemove)
            {
                logger.LogWarning("Removing Url: {Url} after too many errors", url);
                bool successRemove = _feedStates.TryRemove(url, out _);

                if (!successRemove)
                {
                    logger.LogWarning("Failed to remove Url: {Url}", url);
                }
            }
        }


    }
}
