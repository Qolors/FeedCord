using FeedCord.Common;
using FeedCord.Core.Interfaces;

namespace FeedCord.Core.Factories
{
    public interface IDiscordPayloadServiceFactory
    {
        IDiscordPayloadService Create(Config config);
    }
}
