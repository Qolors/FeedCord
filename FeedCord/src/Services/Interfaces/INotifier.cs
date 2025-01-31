using FeedCord.src.Common;

namespace FeedCord.src.Services.Interfaces
{
    public interface INotifier
    {
        Task SendNotificationsAsync(List<Post> newPosts);
    }
}
