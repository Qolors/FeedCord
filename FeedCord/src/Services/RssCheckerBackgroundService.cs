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
        private int delayTime;

        public RssCheckerBackgroundService(
            ILogger<RssCheckerBackgroundService> logger,
            IFeedProcessor feedProcessor,
            INotifier notifier,
            Config config)
        {
            this.logger = logger;
            this.feedProcessor = feedProcessor;
            this.notifier = notifier;
            this.delayTime = config.RssCheckIntervalMinutes;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("[{DateTime.Now}]: Starting Background Process", DateTime.Now);
                await RunRoutineBackgroundProcessAsync();
                logger.LogInformation("[{DateTime.Now}]: Finished Background Process", DateTime.Now);
                await Task.Delay(TimeSpan.FromMinutes(delayTime), stoppingToken);
            }
        }

        private async Task RunRoutineBackgroundProcessAsync()
        {
            try
            {
                var posts = await feedProcessor.CheckForNewPostsAsync();

                if (posts.Count > 0)
                {
                    logger.LogInformation("[{DateTime.Now}]: Found new posts. New post count: [ {Posts.Count} ]", DateTime.Now, posts.Count);
                    await notifier.SendNotificationsAsync(posts);
                }
                else
                {
                    logger.LogInformation("[{DateTime.Now}]: Found no new posts.. Ending background process.", DateTime.Now);
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[{DateTime.Now}]: An error occurred while checking for new posts.", DateTime.Now);
            }
        }
    }
}
