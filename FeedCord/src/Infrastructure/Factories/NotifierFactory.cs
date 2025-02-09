using FeedCord.Common;
using Microsoft.Extensions.DependencyInjection;
using FeedCord.Core.Interfaces;
using FeedCord.Infrastructure.Notifiers;
using FeedCord.Services.Interfaces;

namespace FeedCord.Infrastructure.Factories
{
    public class NotifierFactory : INotifierFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public NotifierFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public INotifier Create(Config config, IDiscordPayloadService discordPayloadService)
        {
            return ActivatorUtilities.CreateInstance<DiscordNotifier>(_serviceProvider, config, discordPayloadService);
        }
    }
}
