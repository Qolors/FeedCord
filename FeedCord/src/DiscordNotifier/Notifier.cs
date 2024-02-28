using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net;

namespace FeedCord.src.DiscordNotifier
{
    internal class Notifier : INotifier
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<INotifier> logger;
        private readonly IDiscordPayloadService discordPayloadService;
        private readonly string webhook;
        private readonly bool forum;
        public Notifier(Config config, IHttpClientFactory httpClientFactory, ILogger<INotifier> logger, IDiscordPayloadService discordPayloadService) 
        {
            this.httpClient = httpClientFactory.CreateClient("Default");
            this.discordPayloadService = discordPayloadService;
            this.webhook = config.DiscordWebhookUrl;
            this.forum = config.Forum;
            this.logger = logger;
        }
        public async Task SendNotificationsAsync(List<Post> newPosts)
        {
            foreach (Post post in newPosts)
            {
                var content = forum ? 
                    discordPayloadService.BuildForumWithPost(post) :
                    discordPayloadService.BuildPayloadWithPost(post);

                if (content is null)
                {
                    logger.LogError("[{CurrentTime}]: Payload Service returned error after attempting to build", DateTime.Now);
                    continue;
                }

                var response = await httpClient.PostAsync(webhook, content);

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    logger.LogInformation("[{CurrentTime}]: Response - Successful: Posted new content to Discord Text Channel at {CurrentTime}", DateTime.Now, DateTime.Now);
                }
                else
                {
                    logger.LogError("Received Status Code - {StatusCode}: Failed post to Discord Channel", response.StatusCode);
                }

                await Task.Delay(10000);
            }
        }
    }
}
