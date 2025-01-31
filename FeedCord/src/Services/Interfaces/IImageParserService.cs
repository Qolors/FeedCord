

namespace FeedCord.src.Services.Interfaces
{
    public interface IImageParserService
    {
        Task<string> TryExtractImageLink(string source);
    }
}
