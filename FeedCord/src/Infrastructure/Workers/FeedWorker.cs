using FeedCord.Common;
using FeedCord.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FeedCord.Infrastructure.Workers
{
    public class FeedWorker : BackgroundService
    {
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ILogger<FeedWorker> _logger;
        private readonly IFeedManager _feedManager;
        private readonly INotifier _notifier;

        private readonly bool _persistent;
        private readonly string _id;
        private readonly int _delayTime;
        private bool _isInitialized;
        

        public FeedWorker(
            IHostApplicationLifetime lifetime,
            ILogger<FeedWorker> logger,
            IFeedManager feedManager,
            INotifier notifier,
            Config config)
        {
            _lifetime = lifetime;
            _logger = logger;
            _feedManager = feedManager;
            _notifier = notifier;
            _delayTime = config.RssCheckIntervalMinutes;
            _id = config.Id;
            _isInitialized = false;
            _persistent = config.PersistenceOnShutdown;

            logger.LogInformation("{id} Created with check interval {Interval} minutes",
                _id, config.RssCheckIntervalMinutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            _lifetime.ApplicationStopping.Register(OnShutdown);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("{id} Starting Background Processing at {CurrentTime}..", _id, DateTime.Now);

                await RunRoutineBackgroundProcessAsync();

                _logger.LogInformation("{id} Finished Background Processing at {CurrentTime}..", _id, DateTime.Now);

                await Task.Delay(TimeSpan.FromMinutes(_delayTime), stoppingToken);
            }
        }

        private async Task RunRoutineBackgroundProcessAsync()
        {
            if (!_isInitialized)
            {
                _logger.LogInformation("{id}: Initializing Url Checks..", _id);
                await _feedManager.InitializeUrlsAsync();
                _isInitialized = true;
            }

            var posts = await _feedManager.CheckForNewPostsAsync();

            if (posts.Count > 0)
            {
                _logger.LogInformation("{id}: Found {PostCount} new posts..", _id, posts.Count);
                await _notifier.SendNotificationsAsync(posts);
            }
        }

        private void OnShutdown()
        {
            if (!_persistent) return;
            
            var data = _feedManager.GetAllFeedData();
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
