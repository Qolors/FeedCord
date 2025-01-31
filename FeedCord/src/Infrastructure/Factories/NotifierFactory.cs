using FeedCord.src.Common;
using Microsoft.Extensions.DependencyInjection;
using FeedCord.src.Core.Interfaces;
using FeedCord.src.Infrastructure.Notifiers;
using FeedCord.src.Services.Interfaces;

namespace FeedCord.src.Infrastructure.Factories
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
