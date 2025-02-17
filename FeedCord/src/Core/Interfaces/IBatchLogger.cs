using FeedCord.Common;

namespace FeedCord.Core.Interfaces;

public interface IBatchLogger
{
    Task ConsumeLogData(LogAggregator logItem);
}