
namespace FeedCord.src.Common.Interfaces
{
    internal interface IRssProcessorService
    {
        Task<Post?> ParseRssFeedAsync(string xmlContent);
        Task<Post?> ParseYoutubeFeedAsync(string channelUrl);
    }
}
