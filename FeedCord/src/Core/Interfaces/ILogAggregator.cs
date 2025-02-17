using System.Collections.Concurrent;
using FeedCord.Common;

namespace FeedCord.Core.Interfaces;

public interface ILogAggregator
{
    Task SendToBatchAsync();
    void SetStartTime(DateTime startTime);
    void SetEndTime(DateTime endTime);
    void SetNewPostCount(int newPostCount);
    void AddLatestUrlPost(string url, Post? post);
    void AddUrlResponse(string url, int status);
    void Reset();
    ConcurrentDictionary<string, int> GetUrlResponses();
}