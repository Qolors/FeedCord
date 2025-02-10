using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using FeedCord.Services.Interfaces;
using System.Collections.Concurrent;

namespace FeedCord.Infrastructure.Http
{
    public class CustomHttpClient : ICustomHttpClient
    {
        // TODO --> Eventually move these to a config file
        private const string USER_MIMICK = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.5112.79 Safari/537.36";
        private const string GOOGLE_FEED_FETCHER = "FeedFetcher-Google";

        private readonly HttpClient _innerClient;
        private readonly ILogger<CustomHttpClient> _logger;
        private readonly SemaphoreSlim _throttle;
        private readonly ConcurrentDictionary<string, string> _userAgentCache;
        public CustomHttpClient(ILogger<CustomHttpClient> logger, HttpClient innerClient, SemaphoreSlim throttle)
        {
            _logger = logger;
            _throttle = throttle;
            _innerClient = innerClient;
            _userAgentCache = new ConcurrentDictionary<string, string>();
        }

        public async Task<HttpResponseMessage> GetAsyncWithFallback(string url)
        {
            await _throttle.WaitAsync();

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (_userAgentCache.ContainsKey(url))
            {
                request.Headers.UserAgent
                    .ParseAdd(_userAgentCache.GetValueOrDefault(url, ""));
            }

            var response = await _innerClient.SendAsync(request);

            _throttle.Release();

            if (!response.IsSuccessStatusCode)
            {
                response = await TryAlternativeAsync(url, response);
            }

            return response;
        }

        public async Task PostAsyncWithFallback(string url, StringContent forumChannelContent, StringContent textChannelContent, bool isForum)
        {
            await _throttle.WaitAsync();

            var response = await _innerClient.PostAsync(url, isForum ? forumChannelContent : textChannelContent);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                _logger.LogInformation("[{CurrentTime}]: Response - Successful: Posted new content to Discord Channel at {CurrentTime}", DateTime.Now, DateTime.Now);
            }
            else
            {
                await _throttle.WaitAsync();

                _logger.LogError("Response Error: {ResponseError}", response.Content.ReadAsStringAsync().Result);

                response = await _innerClient.PostAsync(url, !isForum ? forumChannelContent : textChannelContent);

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    _logger.LogWarning("Successfully posted to Discord Channel after switching channel type - Change Forum Property in Config!!");
                }
                else
                {
                    _logger.LogError("Failed to post to Discord Channel after fallback attempts: {Url}", url);

                }
            }

            _throttle.Release();
        }

        private async Task<HttpResponseMessage> TryAlternativeAsync(string url, HttpResponseMessage oldResponse)
        {
            var uri = new Uri(url);
            var baseUrl = uri.GetLeftPart(UriPartial.Authority);

            //USER MIMICK
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd(USER_MIMICK);

            await _throttle.WaitAsync();

            var response = await _innerClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                _userAgentCache.AddOrUpdate(url, USER_MIMICK, (_, _) => USER_MIMICK);
                _throttle.Release();
                return response;
            }

            //GOOGLE FEED FETCHER
            request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd(GOOGLE_FEED_FETCHER);
            await _throttle.WaitAsync();
            response = await _innerClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _userAgentCache.AddOrUpdate(url, GOOGLE_FEED_FETCHER, (_, _) => GOOGLE_FEED_FETCHER);
                _throttle.Release();
                return response;
            }

            //USERAGENT SCRAPE
            var robotsUrl = new Uri(new Uri(baseUrl), "/robots.txt").AbsoluteUri;
            var userAgents = await GetRobotsUserAgentsAsync(robotsUrl);
            
            if (userAgents.Count > 0)
            {
                foreach (var userAgent in userAgents)
                {
                    request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.UserAgent.ParseAdd(userAgent);
                    request.Headers.Add("Accept", "*/*");
                    await _throttle.WaitAsync();
                    response = await _innerClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        _userAgentCache.AddOrUpdate(url, userAgent, (_, _) => userAgent);
                        _throttle.Release();
                        return response;
                    }
                }
            }

            _logger.LogError("Failed to fetch RSS Feed after fallback attempts: {Url}", url);
            _throttle.Release();
            return oldResponse;
        }

        private async Task<string> FetchRobotsContentAsync(string url)
        {
            try
            {
                await _throttle.WaitAsync();
                return await _innerClient.GetStringAsync(url);
            }
            catch
            {
                return string.Empty;
            }
            finally
            {
                _throttle.Release();
            }
        }

        private async Task<List<string>> GetRobotsUserAgentsAsync(string url)
        {
            var userAgents = new List<string>();

            var robotsContent = await FetchRobotsContentAsync(url);

            if (robotsContent == string.Empty) 
                return userAgents.OrderByDescending(x => x).Distinct().ToList();
            
            var pattern = @"User-agent:\s*(?<agent>.+)";
            var regex = new Regex(pattern);

            var matches = regex.Matches(robotsContent);

            foreach (Match match in matches)
            {
                var userAgent = match.Groups["agent"].Value.Trim();
                if (!string.IsNullOrEmpty(userAgent))
                {
                    userAgents.Add(userAgent);
                }
            }

            return userAgents.OrderByDescending(x => x).Distinct().ToList();
        }
    }
}
