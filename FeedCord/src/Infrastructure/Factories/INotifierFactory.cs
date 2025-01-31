using FeedCord.src.Common;
using FeedCord.src.Core.Interfaces;
using FeedCord.src.Services.Interfaces;

namespace FeedCord.src.Infrastructure.Factories
{
    public interface INotifierFactory
    {
        INotifier Create(Config config, IDiscordPayloadService discordPayloadService);
    }
}
