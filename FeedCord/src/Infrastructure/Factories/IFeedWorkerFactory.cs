using FeedCord.Common;
using FeedCord.Infrastructure.Workers;
using FeedCord.Services.Interfaces;

namespace FeedCord.Infrastructure.Factories
{
    public interface IFeedWorkerFactory
    {
        FeedWorker Create(Config config, IFeedManager feedProcessor, INotifier notifier);
    }
}
