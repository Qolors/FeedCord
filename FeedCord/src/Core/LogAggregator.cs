using System.Collections.Concurrent;
using FeedCord.Common;
using FeedCord.Core.Interfaces;

namespace FeedCord.Core;

public class LogAggregator : ILogAggregator
{
    private IBatchLogger _batchLogger;
    public ConcurrentDictionary<string, int> UrlStatuses { get; set; } = new ConcurrentDictionary<string, int>();
    public ConcurrentDictionary<string, Post?> LatestPosts  { get; set; } = new ConcurrentDictionary<string, Post?>();
    public string InstanceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Post? LatestPost { get; set; }
    public int NewPostCount { get; set; } = 0;
    public LogAggregator(IBatchLogger batchLogger, Config config)
    {
        _batchLogger = batchLogger;
        InstanceId = config.Id;
    }

    public async Task SendToBatchAsync()
    {
        await _batchLogger.ConsumeLogData(this);
    }

    public void SetStartTime(DateTime startTime)
    {
        StartTime = startTime;
    }

    public void SetEndTime(DateTime endTime)
    {
        EndTime = endTime;
    }

    public void SetNewPostCount(int newPostCount)
    {
        NewPostCount = newPostCount;
    }

    public void SetRecentPost(Post? recentPost)
    {
        LatestPost = recentPost;
    }

    public void AddLatestUrlPost(string url, Post? post)
    {
        LatestPosts.TryAdd(url, post);
    }
    public void AddUrlResponse(string url, int response)
    {
        UrlStatuses.TryAdd(url, response);
    }

    public ConcurrentDictionary<string, int> GetUrlResponses()
    {
        return UrlStatuses;
    }

    public void Reset()
    {
        StartTime = default;
        EndTime = default;
        LatestPost = null;
        NewPostCount = 0;
        UrlStatuses.Clear();
        LatestPosts.Clear();
    }
}