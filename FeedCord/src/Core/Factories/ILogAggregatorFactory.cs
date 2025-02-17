using FeedCord.Common;
using FeedCord.Core.Interfaces;

namespace FeedCord.Core.Factories;

public interface ILogAggregatorFactory
{
    ILogAggregator Create(Config config);
}