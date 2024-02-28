
namespace FeedCord.src.Common.Interfaces
{
    public interface IRssProcessorService
    {
        Task<List<Post?>> ParseRssFeedAsync(string xmlContent, int trim);
        Task<Post?> ParseYoutubeFeedAsync(string channelUrl);
    }
}
