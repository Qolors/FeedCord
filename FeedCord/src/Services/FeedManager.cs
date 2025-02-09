using FeedCord.Common;
using FeedCord.Helpers;
using FeedCord.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text;

namespace FeedCord.Services
{
    public class FeedManager : IFeedManager
    {
        private readonly Config _config;
        private readonly ICustomHttpClient _httpClient;
        private readonly ILogger<FeedManager> _logger;
        private readonly IRssParsingService _rssParsingService;
        private readonly Dictionary<string, ReferencePost> _lastRunReference;
        private readonly ConcurrentDictionary<string, FeedState> _feedStates;

        public FeedManager(
            Config config,
            ICustomHttpClient httpClient,
            IRssParsingService rssParsingService,
            ILogger<FeedManager> logger)
        {
            _config = config;
            _httpClient = httpClient;
            _lastRunReference = CsvReader.LoadReferencePosts("feed_dump.csv");
            _rssParsingService = rssParsingService;
            _logger = logger;
            _feedStates = new ConcurrentDictionary<string, FeedState>();
        }
        public async Task<List<Post>> CheckForNewPostsAsync()
        {
            ConcurrentBag<Post> allNewPosts = new();

            var tasks = _feedStates.Select(async (feed) => 
                await CheckSingleFeedAsync(feed.Key, feed.Value, allNewPosts, _config.DescriptionLimit));

            await Task.WhenAll(tasks);

            return allNewPosts.ToList();
        }
        public async Task InitializeUrlsAsync()
        {
            var totalUrls = 0;
            var rssCount = await GetSuccessCount(_config.RssUrls, false);
            var youtubeCount = await GetSuccessCount(_config.YoutubeUrls, true);
            var successCount = 0;
            var id = _config.Id;

            if (_config.YoutubeUrls.Length == 1 && string.IsNullOrEmpty(_config.YoutubeUrls[0]))
            {
                totalUrls = _config.RssUrls.Length;
            }
            else
            {
                totalUrls = _config.RssUrls.Length + _config.YoutubeUrls.Length;
            }

            successCount = rssCount + youtubeCount;

            _logger.LogInformation("{id}: Tested successfully for {UrlCount} out of {TotalUrls} Urls in Configuration File", id, successCount, totalUrls);
        }

        public IReadOnlyDictionary<string, FeedState> GetAllFeedData()
        {
            return _feedStates;
        }
        private async Task<int> GetSuccessCount(string[] urls, bool isYoutube)
        {
            var successCount = 0;

            if (urls.Length == 0 || urls.Length == 1 && string.IsNullOrEmpty(urls[0]))
            {
                return successCount;
            }

            foreach (var url in urls)
            {
                var isSuccess = await TestUrlAsync(url);

                if (!isSuccess) continue;
                
                if (_lastRunReference.TryGetValue(url, out var value))
                {
                    _feedStates.TryAdd(url, new FeedState
                    {
                        IsYoutube = isYoutube,
                        LastPublishDate = value.LastRunDate,
                        ErrorCount = 0
                    });

                    _logger.LogInformation("Successfully initialized Existing URL: {Url}", url);

                    successCount++;

                    continue;
                }

                bool successfulAdd;

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
                    _logger.LogInformation("Successfully initialized URL: {Url}", url);
                }

                else
                {
                    _logger.LogWarning("Failed to initialize URL: {Url}", url);
                }
            }

            return successCount;
        }
        private async Task<bool> TestUrlAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetAsyncWithFallback(url);
                _logger.LogInformation($"Status Code: {(int)response.StatusCode} {response.StatusCode}");

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
            _logger.LogInformation("Checking if any new posts for {Url}...", url);

            List<Post?> posts;
            try
            {
                posts = feedState.IsYoutube ?
                    await FetchYoutubeAsync(url) :
                    await FetchRssAsync(url, trim);
            }
            catch (Exception ex)
            {
                HandleFeedError(url, feedState, ex);
                return;
            }

            //var freshlyFetched = posts.Where(p => p.PublishDate > feedState.LastPublishDate).ToList();
            var freshlyFetched = posts;
            if (freshlyFetched.Any())
            {
                feedState.LastPublishDate = freshlyFetched.Max(p => p.PublishDate);
                feedState.ErrorCount = 0;

                foreach (var post in freshlyFetched)
                {
                    if (post is null)
                    {
                        _logger.LogWarning("Failed to parse a post from {Url}", url);
                        continue;
                    }
                    newPosts.Add(post);
                }
            }
        }
        private async Task<List<Post?>> FetchYoutubeAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetAsyncWithFallback(url);

                response.EnsureSuccessStatusCode();

                var xmlContent = await GetResponseContentAsync(response);

                var post = await _rssParsingService.ParseYoutubeFeedAsync(xmlContent);

                return post == null ? 
                    new List<Post?>() : 
                    new List<Post?> { post };

            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning("Failed to fetch or process the RSS feed from {Url}: Response Ended Prematurely - Skipping Url", url);
                return new List<Post?>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "An unexpected error occurred while checking the RSS feed from {Url}", url);
                return new List<Post?>();
            }
        }

        private async Task<List<Post?>> FetchRssAsync(string url, int trim)
        {
            try
            {
                var response = await _httpClient.GetAsyncWithFallback(url);

                response.EnsureSuccessStatusCode();

                var xmlContent = await GetResponseContentAsync(response);

                return await _rssParsingService.ParseRssFeedAsync(xmlContent, trim);

            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning("Failed to fetch or process the RSS feed from {Url}: Response Ended Prematurely - Skipping Url", url);
                return new List<Post?>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "An unexpected error occurred while checking the RSS feed from {Url}", url);
                return new List<Post?>();
            }
        }
        private async Task<string> GetResponseContentAsync(HttpResponseMessage response)
        {
            if (response.Content.Headers.ContentEncoding.Contains("gzip"))
            {
                await using var decompressedStream = new GZipStream(await response.Content.ReadAsStreamAsync(), CompressionMode.Decompress);
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
            _logger.LogWarning(ex, "Failed to fetch or parse feed {Url}", url);
            feedState.ErrorCount++;

            if (feedState.ErrorCount < 3 || !_config.EnableAutoRemove) return;
            
            _logger.LogWarning("Removing Url: {Url} after too many errors", url);
            var successRemove = _feedStates.TryRemove(url, out _);

            if (!successRemove)
            {
                _logger.LogWarning("Failed to remove Url: {Url}", url);
            }
        }


    }
}
