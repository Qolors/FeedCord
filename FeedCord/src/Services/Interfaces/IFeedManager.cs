using FeedCord.Common;

namespace FeedCord.Services.Interfaces
{
    public interface IFeedManager
    {
        Task<List<Post>> CheckForNewPostsAsync();
        Task InitializeUrlsAsync();
        IReadOnlyDictionary<string, FeedState> GetAllFeedData();
    }
}
