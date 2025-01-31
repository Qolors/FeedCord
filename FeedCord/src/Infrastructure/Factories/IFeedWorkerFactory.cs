using FeedCord.src.Common;
using FeedCord.src.Infrastructure.Workers;
using FeedCord.src.Services.Interfaces;

namespace FeedCord.src.Infrastructure.Factories
{
    public interface IFeedWorkerFactory
    {
        FeedWorker Create(Config config, IFeedManager feedProcessor, INotifier notifier);
    }
}
