using FeedCord.src.Common;
using FeedCord.src.Services;
using FeedCord.src.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FeedCord.src.Infrastructure.Workers
{
    public class FeedWorker : BackgroundService
    {
        private readonly IHostApplicationLifetime lifetime;
        private readonly ILogger<FeedWorker> logger;
        private readonly IFeedManager feedManager;
        private readonly INotifier notifier;

        private readonly int delayTime;
        private bool isInitialized = false;
        private readonly string id;

        public FeedWorker(
            IHostApplicationLifetime lifetime,
            ILogger<FeedWorker> logger,
            IFeedManager feedManager,
            INotifier notifier,
            Config config)
        {
            this.lifetime = lifetime;
            this.logger = logger;
            this.feedManager = feedManager;
            this.notifier = notifier;
            delayTime = config.RssCheckIntervalMinutes;
            id = config.Id;

            logger.LogInformation("{id} Created with check interval {Interval} minutes",
                id, config.RssCheckIntervalMinutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            lifetime.ApplicationStopping.Register(OnShutdown);

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
                await feedManager.InitializeUrlsAsync();
                isInitialized = true;
            }

            var posts = await feedManager.CheckForNewPostsAsync();

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

        private void OnShutdown()
        {
            var data = feedManager.GetAllFeedData();
            SaveDataToCsv(data);
        }

        private void SaveDataToCsv(IReadOnlyDictionary<string, FeedState> data)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "feed_dump.csv");
            using var writer = new StreamWriter(filePath);

            foreach (var (key, value) in data)
            {
                writer.WriteLine($"{key},{value.IsYoutube},{DateTime.Now}");
            }
        }
    }
}
