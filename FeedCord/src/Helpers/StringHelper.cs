using System.Text.RegularExpressions;

namespace FeedCord.src.Helpers
{
    public static partial class StringHelper
    {
        public static string StripTags(string source)
        {
            return GeneratedStripTagsRegex().Replace(source, string.Empty);
        }

        [GeneratedRegex("<.*?>")]
        private static partial Regex GeneratedStripTagsRegex();
    }
}
