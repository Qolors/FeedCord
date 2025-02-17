using FeedCord.Common;
using FeedCord.Core.Interfaces;
using FeedCord.Services.Interfaces;

namespace FeedCord.Services.Factories
{
    public interface IFeedManagerFactory
    {
        IFeedManager Create(Config config, ILogAggregator logAggregator);
    }
}
