using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;

namespace FeedCord.src.Factories.Interfaces
{
    public interface IDiscordPayloadServiceFactory
    {
        IDiscordPayloadService Create(Config config);
    }
}
