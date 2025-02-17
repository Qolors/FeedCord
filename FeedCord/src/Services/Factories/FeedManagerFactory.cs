using FeedCord.Common;
using FeedCord.Core.Interfaces;
using FeedCord.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FeedCord.Services.Factories
{
    public class FeedManagerFactory : IFeedManagerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public FeedManagerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IFeedManager Create(Config config, ILogAggregator logAggregator)
        {
            return ActivatorUtilities.CreateInstance<FeedManager>(_serviceProvider, config, logAggregator);
        }
    }
}
