using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;
using FeedCord.src.DiscordNotifier;
using FeedCord.src.RssReader;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FeedCord.src.Services
{
    internal class RssCheckerBackgroundService : BackgroundService
    {
        private readonly ILogger<RssCheckerBackgroundService> logger;
        private readonly IFeedProcessor feedProcessor;
        private readonly INotifier notifier;

        public RssCheckerBackgroundService(
            ILogger<RssCheckerBackgroundService> logger,
            IFeedProcessor feedProcessor,
            INotifier notifier)
        {
            this.logger = logger;
            this.feedProcessor = feedProcessor;
            this.notifier = notifier;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await RunRoutineBackgroundProcessAsync();
                Console.WriteLine("Completed Background Process");
                await Task.Delay(TimeSpan.FromMinutes(10));
            }
        }

        private async Task RunRoutineBackgroundProcessAsync()
        {
            try
            {
                var posts = await feedProcessor.CheckForNewPostsAsync();

                if (posts.Count > 0)
                {
                    await notifier.SendNotificationsAsync(posts);
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while checking for new posts.");
            }
        }
    }
}
