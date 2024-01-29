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

        public async Task<IFeedProcessor> Create(Config config)
        {
            //TODO --> CREATE ASYNC INITIALIZATION FOR FEEDPROCESSOR CREATION 

            FeedProcessor feedProcessor = await FeedProcessor.CreateAsync(config).Result;

            return ActivatorUtilities.CreateInstance<FeedProcessor>(_serviceProvider, config);
        }
    }
}
