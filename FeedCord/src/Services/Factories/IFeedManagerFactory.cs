using FeedCord.src.Common;
using FeedCord.src.Services.Interfaces;

namespace FeedCord.src.Services.Factories
{
    public interface IFeedManagerFactory
    {
        IFeedManager Create(Config config);
    }
}
