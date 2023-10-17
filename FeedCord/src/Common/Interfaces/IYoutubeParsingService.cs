namespace FeedCord.src.Common.Interfaces
{
    internal interface IYoutubeParsingService
    {
        Task<Post?> GetXmlUrlAndFeed(string url);
    }
}