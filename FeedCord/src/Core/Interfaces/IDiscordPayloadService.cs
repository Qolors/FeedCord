using FeedCord.Common;

namespace FeedCord.Core.Interfaces
{
    public interface IDiscordPayloadService
    {
        StringContent BuildForumWithPost(Post post);
        StringContent BuildPayloadWithPost(Post post);
    }
}
