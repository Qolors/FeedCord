

namespace FeedCord.src.Common.Interfaces
{
    public interface IFeedProcessor
    {
        Task<List<Post>> CheckForNewPostsAsync();

        Task InitializeUrlsAsync();
    }
}
