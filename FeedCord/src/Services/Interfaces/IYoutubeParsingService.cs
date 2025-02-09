using FeedCord.Common;

namespace FeedCord.Services.Interfaces
{
    public interface IYoutubeParsingService
    {
        Task<Post?> GetXmlUrlAndFeed(string url);
    }
}