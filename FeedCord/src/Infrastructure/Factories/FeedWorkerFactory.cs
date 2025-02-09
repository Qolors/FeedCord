using FeedCord.Common;
using FeedCord.Infrastructure.Workers;
using FeedCord.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FeedCord.Infrastructure.Factories
{
    public class FeedWorkerFactory : IFeedWorkerFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FeedWorkerFactory> _logger;

        public FeedWorkerFactory(IServiceProvider serviceProvider, ILogger<FeedWorkerFactory> rssCheckerLogger)
        {
            _serviceProvider = serviceProvider;
            _logger = rssCheckerLogger;
        }

        public FeedWorker Create(Config config, IFeedManager feedProcessor, INotifier notifier)
        {
            _logger.LogInformation("Creating new RssCheckerBackgroundService instance for {Id}", config.Id);
            return ActivatorUtilities.CreateInstance<FeedWorker>(_serviceProvider, config, feedProcessor, notifier);
        }
    }
}
