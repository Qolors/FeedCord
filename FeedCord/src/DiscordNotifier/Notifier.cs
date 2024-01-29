using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;
using FeedCord.src.Services;
using Microsoft.Extensions.Logging;
using System.Net;

namespace FeedCord.src.DiscordNotifier
{
    internal class Notifier : INotifier
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<INotifier> logger;
        private readonly string webhook;
        private readonly bool forum;
        private DiscordPayloadService discordPayloadService;
        public Notifier(Config config, IHttpClientFactory httpClientFactory, ILogger<INotifier> logger, DiscordPayloadService discordPayloadService) 
        {
            this.httpClient = httpClientFactory.CreateClient("Default");
            this.logger = logger;
            this.webhook = config.DiscordWebhookUrl;
            this.forum = config.Forum;
            this.discordPayloadService = discordPayloadService;
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

                await Task.Delay(1000);
            }
        }
    }
}
