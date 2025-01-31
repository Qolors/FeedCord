using FeedCord.src.Common;
using FeedCord.src.Core.Interfaces;
using FeedCord.src.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace FeedCord.src.Infrastructure.Notifiers
{
    internal class DiscordNotifier : INotifier
    {
        private readonly ICustomHttpClient httpClient;
        private readonly ILogger<INotifier> logger;
        private readonly IDiscordPayloadService discordPayloadService;
        private readonly string webhook;
        private readonly bool forum;
        public DiscordNotifier(Config config, ICustomHttpClient httpClient, ILogger<INotifier> logger, IDiscordPayloadService discordPayloadService)
        {
            this.httpClient = httpClient;
            this.discordPayloadService = discordPayloadService;
            webhook = config.DiscordWebhookUrl;
            forum = config.Forum;
            this.logger = logger;
        }
        public async Task SendNotificationsAsync(List<Post> newPosts)
        {
            foreach (Post post in newPosts)
            {
                // TODO --> This is to dynamically handle users setting the forum flag to true or false incorrectly
                // May revisit when config setup is more robust
                var forumChannelContent = discordPayloadService.BuildForumWithPost(post);
                var textChannelContent = discordPayloadService.BuildPayloadWithPost(post);

                if (textChannelContent is null || forumChannelContent is null)
                {
                    logger.LogError("[{CurrentTime}]: Payload Service returned error after attempting to build", DateTime.Now);
                    continue;
                }

                await httpClient.PostAsyncWithFallback(webhook, forumChannelContent, textChannelContent, forum);

                // TODO --> This is to prevent rate limiting from Discord API - Simple but eventually want to handle this in our CustomHttpClient
                await Task.Delay(10000);
            }
        }
    }
}
