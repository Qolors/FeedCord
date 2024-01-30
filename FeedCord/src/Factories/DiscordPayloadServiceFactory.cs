using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;
using FeedCord.src.Factories.Interfaces;
using FeedCord.src.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FeedCord.src.Factories
{
    public class DiscordPayloadServiceFactory : IDiscordPayloadServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DiscordPayloadServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public IDiscordPayloadService Create(Config config)
        {
            return ActivatorUtilities.CreateInstance<DiscordPayloadService>(_serviceProvider, config);
        }
    }
}
