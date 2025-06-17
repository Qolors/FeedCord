using FeedCord.Common;

namespace FeedCord.Services.Helpers;

public static class FilterConfigs
{
    public static bool GetFilterSuccess(Post post, params string[] filters)
    {
        if (filters == null || filters.Length == 0)
            return true;

        foreach (var filter in filters)
        {
            // filter for label by looking for the prefix "label:" in filter settings
            if (filter.StartsWith("label:", StringComparison.OrdinalIgnoreCase))
            {
                var labelToFind = filter.Substring("label:".Length);
                if (post.Labels != null && post.Labels.Any(l => l.Equals(labelToFind, StringComparison.OrdinalIgnoreCase)))
                    return true;
            }
            else
            {
                if ((post.Title != null && post.Title.Contains(filter, StringComparison.OrdinalIgnoreCase)) ||
                    (post.Description != null && post.Description.Contains(filter, StringComparison.OrdinalIgnoreCase)))
                    return true;
            }
        }
        return false;
    }
}