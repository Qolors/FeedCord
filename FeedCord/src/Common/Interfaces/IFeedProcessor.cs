

namespace FeedCord.src.Common.Interfaces
{
    internal interface IFeedProcessor
    {
        Task<List<Post>> CheckForNewPostsAsync();
    }
}
