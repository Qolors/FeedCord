using FeedCord.src.Common.Interfaces;
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
                logger.LogWarning("Error extracting the Image URL from source: {Source}", source);
                logger.LogInformation("Using Fallback Image in Configuration at {CurrentTime}", DateTime.Now);
                return string.Empty;
            }
        }
    }
}
