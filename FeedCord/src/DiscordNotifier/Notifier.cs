using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;
using FeedCord.src.Services;
using Microsoft.Extensions.Logging;

namespace FeedCord.src.DiscordNotifier
{
    internal class Notifier : INotifier
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<INotifier> logger;
        private readonly string webhook;
        public Notifier(Config config, IHttpClientFactory httpClientFactory, ILogger<INotifier> logger) 
        {
            httpClient = httpClientFactory.CreateClient();
            this.logger = logger;
            webhook = config.Webhook;
            DiscordPayloadService.SetConfig(config);
        }
        public async Task SendNotificationsAsync(List<Post> newPosts)
        {
            foreach (Post post in newPosts)
            {
                var content = DiscordPayloadService.BuildPayloadWithPost(post);

                if (content is null)
                {
                    logger.LogError("[{DateTime.Now}]: Payload Service returned null after attempting to build", DateTime.Now);
                    continue;
                }

                var response = await httpClient.PostAsync(webhook, content);

                logger.LogInformation("[{DateTime.Now}]: {Response.EnsureSuccessStatusCode()}", DateTime.Now, response.EnsureSuccessStatusCode());

                await Task.Delay(1000);
            }
        }
    }
}
