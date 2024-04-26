using FeedCord.src.Common.Interfaces;
using FeedCord.src.Helpers;
using Microsoft.Extensions.Logging;

namespace FeedCord.src.Services
{
    public class OpenGraphService : IOpenGraphService
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<OpenGraphService> logger;

        public OpenGraphService(IHttpClientFactory httpClientFactory, ILogger<OpenGraphService> logger)
        {
            this.httpClient = httpClientFactory.CreateClient("Default");
            this.logger = logger;
        }

        public async Task<string> ExtractImageUrl(string source)
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;

            try
            {
                var httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.GetAsync(source);
                response.EnsureSuccessStatusCode();

                string htmlContent = await response.Content.ReadAsStringAsync();

                var htmlDocument = new HtmlAgilityPack.HtmlDocument();
                htmlDocument.LoadHtml(htmlContent);

                var ogImage = htmlDocument
                    .DocumentNode
                    .SelectSingleNode("//meta[@property='og:image']")?
                    .GetAttributeValue("content", string.Empty);

                return ogImage ?? string.Empty;
            }
            catch (Exception ex)
            {
                logger.LogWarning("Didn't find Image URL from source: {Source} - Using Fallback Image in Configuration", source);
                return string.Empty;
            }
        }
    }
}
