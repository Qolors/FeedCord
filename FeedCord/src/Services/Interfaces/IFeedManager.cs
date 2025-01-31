using FeedCord.src.Common;

namespace FeedCord.src.Services.Interfaces
{
    public interface IFeedManager
    {
        Task<List<Post>> CheckForNewPostsAsync();
        Task InitializeUrlsAsync();
        IReadOnlyDictionary<string, FeedState> GetAllFeedData();
    }
}
