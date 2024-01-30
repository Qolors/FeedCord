using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;
using FeedCord.src.Factories.Interfaces;
using FeedCord.src.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FeedCord.src.Factories
{
    public class RssCheckerBackgroundServiceFactory : IRssCheckerBackgroundServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RssCheckerBackgroundServiceFactory> _logger;

        public RssCheckerBackgroundServiceFactory(IServiceProvider serviceProvider, ILogger<RssCheckerBackgroundServiceFactory> rssCheckerLogger)
        {
            _serviceProvider = serviceProvider;
            _logger = rssCheckerLogger;
        }

        public RssCheckerBackgroundService Create(Config config, IFeedProcessor feedProcessor, INotifier notifier)
        {
            _logger.LogInformation("Creating new RssCheckerBackgroundService instance for {Id}", config.Id);
            return ActivatorUtilities.CreateInstance<RssCheckerBackgroundService>(_serviceProvider, config, feedProcessor, notifier);
        }
    }
}
