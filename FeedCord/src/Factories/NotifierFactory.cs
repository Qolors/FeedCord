using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;
using FeedCord.src.DiscordNotifier;
using FeedCord.src.Factories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FeedCord.src.Factories
{
    public class NotifierFactory : INotifierFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public NotifierFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public INotifier Create(Config config)
        {
            return ActivatorUtilities.CreateInstance<Notifier>(_serviceProvider, config);
        }
    }
}
