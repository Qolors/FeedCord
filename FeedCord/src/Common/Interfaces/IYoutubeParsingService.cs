namespace FeedCord.src.Common.Interfaces
{
    public interface IYoutubeParsingService
    {
        Task<Post?> GetXmlUrlAndFeed(string url);
    }
}