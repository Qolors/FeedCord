using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;
using FeedCord.src.Factories.Interfaces;
using FeedCord.src.RssReader;
using Microsoft.Extensions.DependencyInjection;

namespace FeedCord.src.Factories
{
    public class FeedProcessorFactory : IFeedProcessorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public FeedProcessorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IFeedProcessor Create(Config config)
        {
            return ActivatorUtilities.CreateInstance<FeedProcessor>(_serviceProvider, config);
        }
    }
}
