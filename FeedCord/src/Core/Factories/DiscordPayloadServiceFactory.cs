using FeedCord.Common;
using FeedCord.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FeedCord.Core.Factories
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
