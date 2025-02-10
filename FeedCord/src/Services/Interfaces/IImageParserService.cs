

namespace FeedCord.Services.Interfaces
{
    public interface IImageParserService
    {
        Task<string?> TryExtractImageLink(string pageUrl, string xmlSource);
    }
}
