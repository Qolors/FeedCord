using FeedCord.Common;

namespace FeedCord.Services.Interfaces
{
    public interface INotifier
    {
        Task SendNotificationsAsync(List<Post> newPosts);
    }
}
