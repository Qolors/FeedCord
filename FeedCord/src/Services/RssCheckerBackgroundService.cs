using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FeedCord.src.Services
{
    public class RssCheckerBackgroundService : BackgroundService
    {
        private readonly ILogger<RssCheckerBackgroundService> logger;
        private readonly IFeedProcessor feedProcessor;
        private readonly INotifier notifier;
        private readonly int delayTime;
        private bool isInitialized = false;
        private readonly string id;

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
            this.id = config.Id;

            logger.LogInformation("{id} Created with check interval {Interval} minutes",
                this.id, config.RssCheckIntervalMinutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("{id} Starting Background Processing at {CurrentTime}..", id, DateTime.Now);

                await RunRoutineBackgroundProcessAsync();

                logger.LogInformation("{id} Finished Background Processing at {CurrentTime}..", id, DateTime.Now);

                await Task.Delay(TimeSpan.FromMinutes(delayTime), stoppingToken);
            }
        }

        private async Task RunRoutineBackgroundProcessAsync()
        {
            if (!isInitialized)
            {
                logger.LogInformation("{id}: Initializing Url Checks..", id);
                await feedProcessor.InitializeUrlsAsync();
                isInitialized = true;
            }

            var posts = await feedProcessor.CheckForNewPostsAsync();

            if (posts.Count > 0)
            {
                logger.LogInformation("{id}: Found {PostCount} new posts..", id, posts.Count);
                await notifier.SendNotificationsAsync(posts);
            }
            else
            {
                logger.LogInformation("{id}: Found no new posts. Ending background process..", id);
            }
        }
    }
}
