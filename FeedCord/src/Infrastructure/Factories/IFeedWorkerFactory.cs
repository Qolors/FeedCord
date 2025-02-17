using FeedCord.Common;
using FeedCord.Core.Interfaces;
using FeedCord.Infrastructure.Workers;
using FeedCord.Services.Interfaces;

namespace FeedCord.Infrastructure.Factories
{
    public interface IFeedWorkerFactory
    {
        FeedWorker Create(Config config, ILogAggregator loggerAggregator, IFeedManager feedProcessor, INotifier notifier);
    }
}
