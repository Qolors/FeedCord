using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using FeedCord.src.Services.Interfaces;
using System.Collections.Concurrent;

namespace FeedCord.src.Infrastructure.Http
{
    public class CustomHttpClient : ICustomHttpClient
    {
        // TODO --> Eventually move these to a config file
        private const string USER_MIMICK = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.5112.79 Safari/537.36";
        private const string GOOGLE_FEED_FETCHER = "FeedFetcher-Google";

        private readonly HttpClient _innerClient;
        private readonly ILogger<CustomHttpClient> logger;
        private readonly SemaphoreSlim throttle;
        private ConcurrentDictionary<string, string> userAgentCache;
        public CustomHttpClient(ILogger<CustomHttpClient> logger, HttpClient innerClient, SemaphoreSlim throttle)
        {
            this.logger = logger;
            this.throttle = throttle;
            _innerClient = innerClient;
            userAgentCache = new();
        }

        public async Task<HttpResponseMessage> GetAsyncWithFallback(string url)
        {
            await throttle.WaitAsync();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

            if (userAgentCache.ContainsKey(url))
            {
                request.Headers.UserAgent
                    .ParseAdd(userAgentCache
                    .TryGetValue(url, out string? userAgent) ? userAgent : "");
            }

            var response = await _innerClient.SendAsync(request);

            throttle.Release();

            if (!response.IsSuccessStatusCode)
            {
                response = await TryAlternativeAsync(url, response);
            }

            return response;
        }

        public async Task PostAsyncWithFallback(string url, StringContent forumChannelContent, StringContent textChannelContent, bool isForum)
        {
            await throttle.WaitAsync();

            var response = await _innerClient.PostAsync(url, isForum ? forumChannelContent : textChannelContent);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                logger.LogInformation("[{CurrentTime}]: Response - Successful: Posted new content to Discord Channel at {CurrentTime}", DateTime.Now, DateTime.Now);
            }
            else
            {
                string channelType = isForum ? "Forum" : "Text";

                await throttle.WaitAsync();

                response = await _innerClient.PostAsync(url, !isForum ? forumChannelContent : textChannelContent);

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    logger.LogWarning("Successfully posted to Discord Channel after switching channel type - Change Forum Property in Config!!");
                }
                else
                {
                    logger.LogError("Failed to post to Discord Channel after fallback attempts: {Url}", url);
                }
            }

            throttle.Release();
        }

        private async Task<HttpResponseMessage> TryAlternativeAsync(string url, HttpResponseMessage oldResponse)
        {
            Uri uri = new Uri(url);
            string baseUrl = uri.GetLeftPart(UriPartial.Authority);

            //USER MIMICK
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd(USER_MIMICK);
            
            await throttle.WaitAsync();
            HttpResponseMessage response = await _innerClient.SendAsync(request);


            if (response.IsSuccessStatusCode)
            {
                userAgentCache.AddOrUpdate(url, USER_MIMICK, (key, oldValue) => USER_MIMICK);
                throttle.Release();
                return response;
            }

            //GOOGLE FEED FETCHER
            request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd(GOOGLE_FEED_FETCHER);
            await throttle.WaitAsync();
            response = await _innerClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                userAgentCache.AddOrUpdate(url, GOOGLE_FEED_FETCHER, (key, oldValue) => GOOGLE_FEED_FETCHER);
                throttle.Release();
                return response;
            }

            //USERAGENT SCRAPE
            string robotsUrl = new Uri(new Uri(baseUrl), "/robots.txt").AbsoluteUri;
            List<string> userAgents = await GetRobotsUserAgentsAsync(robotsUrl);
            if (userAgents != null && userAgents.Count > 0)
            {
                foreach (var userAgent in userAgents)
                {
                    request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.UserAgent.ParseAdd(userAgent);
                    request.Headers.Add("Accept", "*/*");
                    await throttle.WaitAsync();
                    response = await _innerClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        userAgentCache.AddOrUpdate(url, userAgent, (key, oldValue) => userAgent);
                        throttle.Release();
                        return response;
                    }
                }
            }

            logger.LogError("Failed to fetch RSS Feed after fallback attempts: {Url}", url);
            throttle.Release();
            return oldResponse;
        }

        private async Task<string> FetchRobotsContentAsync(string url)
        {
            try
            {
                await throttle.WaitAsync();
                return await _innerClient.GetStringAsync(url);
            }
            catch
            {
                return string.Empty;
            }
            finally
            {
                throttle.Release();
            }
        }

        private async Task<List<string>> GetRobotsUserAgentsAsync(string url)
        {
            List<string> userAgents = new List<string>();

            string robotsContent = await FetchRobotsContentAsync(url);

            if (robotsContent != string.Empty)
            {
                string pattern = @"User-agent:\s*(?<agent>.+)";
                Regex regex = new Regex(pattern);

                MatchCollection matches = regex.Matches(robotsContent);

                foreach (Match match in matches)
                {
                    string userAgent = match.Groups["agent"].Value.Trim();
                    if (!string.IsNullOrEmpty(userAgent))
                    {
                        userAgents.Add(userAgent);
                    }
                }
            }

            return userAgents.OrderByDescending(x => x).Distinct().ToList();
        }
    }
}
