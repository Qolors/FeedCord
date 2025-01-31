using FeedCord.src.Common;

namespace FeedCord.src.Services.Interfaces
{
    public interface IYoutubeParsingService
    {
        Task<Post?> GetXmlUrlAndFeed(string url);
    }
}