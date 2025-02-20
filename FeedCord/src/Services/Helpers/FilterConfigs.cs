using FeedCord.Common;

namespace FeedCord.Services.Helpers;

public static class FilterConfigs
{
    public static bool GetFilterSuccess(Post post, string[] filterWords)
    {
        var titleLower = post.Title.ToLower();
        var descLower = post.Description.ToLower();

        return filterWords.Any(word => post.Title.Contains(word, StringComparison.OrdinalIgnoreCase))
               || filterWords.Any(word => post.Description.Contains(word, StringComparison.OrdinalIgnoreCase));
    }
}