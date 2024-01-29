using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;
using FeedCord.src.Factories.Interfaces;
using FeedCord.src.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FeedCord.src.Factories
{
    public class RssCheckerBackgroundServiceFactory : IRssCheckerBackgroundServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public RssCheckerBackgroundServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public RssCheckerBackgroundService Create(Config config, IFeedProcessor feedProcessor, INotifier notifier)
        {
            return ActivatorUtilities.CreateInstance<RssCheckerBackgroundService>(_serviceProvider, config, feedProcessor, notifier);
        }
    }
}
