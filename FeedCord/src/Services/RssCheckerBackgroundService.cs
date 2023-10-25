using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;
using FeedCord.src.DiscordNotifier;
using FeedCord.src.RssReader;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FeedCord.src.Services
{
    internal class RssCheckerBackgroundService : BackgroundService
    {
        private readonly ILogger<RssCheckerBackgroundService> logger;
        private readonly IFeedProcessor feedProcessor;
        private readonly INotifier notifier;
        private readonly int delayTime;

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
                logger.LogInformation("Starting Background Process at {CurrentTime}..", DateTime.Now);
                await RunRoutineBackgroundProcessAsync();
                logger.LogInformation("Finished Background Process at {CurrentTime}..", DateTime.Now);
                await Task.Delay(TimeSpan.FromMinutes(delayTime), stoppingToken);
            }
        }

        private async Task RunRoutineBackgroundProcessAsync()
        {
            var posts = await feedProcessor.CheckForNewPostsAsync();

            if (posts.Count > 0)
            {
                logger.LogInformation("Found {PostCount} new posts..", posts.Count);
                await notifier.SendNotificationsAsync(posts);
            }
            else
            {
                logger.LogInformation("Found no new posts. Ending background process..");
            }
        }
    }
}
