

namespace FeedCord.Services.Interfaces
{
    public interface ICustomHttpClient
    {
        Task<HttpResponseMessage> GetAsyncWithFallback(string url);
        Task PostAsyncWithFallback(string url, StringContent forumChannelContent, StringContent textChannelContent, bool isForum);
    }
}
