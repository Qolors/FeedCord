using FeedCord.src.Common;
using FeedCord.src.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FeedCord.src.Services.Factories
{
    public class FeedManagerFactory : IFeedManagerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public FeedManagerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IFeedManager Create(Config config)
        {
            return ActivatorUtilities.CreateInstance<FeedManager>(_serviceProvider, config);
        }
    }
}
