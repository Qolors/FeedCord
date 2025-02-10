using FeedCord.Common;
using FeedCord.Core.Interfaces;
using FeedCord.Services.Interfaces;

namespace FeedCord.Infrastructure.Factories
{
    public interface INotifierFactory
    {
        INotifier Create(Config config, IDiscordPayloadService discordPayloadService);
    }
}
