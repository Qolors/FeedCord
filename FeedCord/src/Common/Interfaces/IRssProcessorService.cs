
namespace FeedCord.src.Common.Interfaces
{
    internal interface IRssProcessorService
    {
        Task<Post?> ParseRssFeedAsync(string xmlContent, int trim);
        Task<Post?> ParseYoutubeFeedAsync(string channelUrl);
    }
}
