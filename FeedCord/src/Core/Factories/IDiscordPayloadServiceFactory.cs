using FeedCord.src.Common;
using FeedCord.src.Core.Interfaces;

namespace FeedCord.src.Core.Factories
{
    public interface IDiscordPayloadServiceFactory
    {
        IDiscordPayloadService Create(Config config);
    }
}
