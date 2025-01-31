using FeedCord.src.Common;

namespace FeedCord.src.Core.Interfaces
{
    public interface IDiscordPayloadService
    {
        StringContent BuildForumWithPost(Post post);
        StringContent BuildPayloadWithPost(Post post);
    }
}
