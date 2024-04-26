using System.Text.RegularExpressions;


namespace FeedCord.src.Helpers
{
    internal class CustomHttpClient : HttpClient
    {
        public CustomHttpClient()
        {
        }

        public new async Task<HttpResponseMessage> GetAsync(string url)
        {
            var response = await base.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                response = await TryAlternativeAsync(url, response);
            }

            return response;
        }

        //try different ways if it fails
        private async Task<HttpResponseMessage> TryAlternativeAsync(string url, HttpResponseMessage oldResponse)
        {
            Uri uri = new Uri(url);
            string baseUrl = uri.GetLeftPart(UriPartial.Authority);

            string robotsUrl = new Uri(new Uri(baseUrl), "/robots.txt").AbsoluteUri;

            List<string> userAgents = await GetRobotsUserAgentsAsync(robotsUrl);

            if (userAgents != null && userAgents.Count > 0)
            {
                foreach (var userAgent in userAgents)
                {
                    HttpClient httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add("Accept", "*/*");
                    HttpResponseMessage response = await httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        return response;
                    }
                }
            }
            return oldResponse;
        }

        private async Task<string> FetchRobotsContentAsync(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    return await client.GetStringAsync(url);
                }
            }
            catch
            {
                return string.Empty;
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
