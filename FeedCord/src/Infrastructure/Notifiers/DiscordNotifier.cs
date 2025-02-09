using FeedCord.Common;
using FeedCord.Core.Interfaces;
using FeedCord.Services.Interfaces;

namespace FeedCord.Infrastructure.Notifiers
{
    internal class DiscordNotifier : INotifier
    {
        private readonly ICustomHttpClient _httpClient;
        private readonly IDiscordPayloadService _discordPayloadService;
        private readonly string _webhook;
        private readonly bool _forum;
        public DiscordNotifier(Config config, ICustomHttpClient httpClient, IDiscordPayloadService discordPayloadService)
        {
            _httpClient = httpClient;
            _discordPayloadService = discordPayloadService;
            _webhook = config.DiscordWebhookUrl;
            _forum = config.Forum;
        }
        public async Task SendNotificationsAsync(List<Post> newPosts)
        {
            foreach (var post in newPosts)
            {
                // TODO --> This is to dynamically handle users setting the forum flag to true or false incorrectly
                // May revisit when config setup is more robust
                var forumChannelContent = _discordPayloadService.BuildForumWithPost(post);
                var textChannelContent = _discordPayloadService.BuildPayloadWithPost(post);

                await _httpClient.PostAsyncWithFallback(_webhook, forumChannelContent, textChannelContent, _forum);

                // TODO --> This is to prevent rate limiting from Discord API - Simple but eventually want to handle this in our CustomHttpClient
                await Task.Delay(10000);
            }
        }
    }
}
