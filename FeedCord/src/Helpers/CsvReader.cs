using FeedCord.src.Common;
using System.Globalization;

namespace FeedCord.src.Helpers
{
    public static class CsvReader
    {
        public static Dictionary<string, ReferencePost> LoadReferencePosts(string filePath)
        {
            var dictionary = new Dictionary<string, ReferencePost>();

            if (!File.Exists(filePath))
            {
                return dictionary;
            }

            try
            {
                using var reader = new StreamReader(filePath);

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split(',');

                    if (parts.Length < 3)
                    {
                        continue;
                    }

                    var url = parts[0].Trim();
                    if (!bool.TryParse(parts[1], out var isYoutube))
                    {
                        continue;
                    }

                    if (!DateTime.TryParse(parts[2], CultureInfo.InvariantCulture, DateTimeStyles.None, out var lastRunDate))
                    {
                        continue;
                    }

                    dictionary[url] = new ReferencePost
                    {
                        IsYoutube = isYoutube,
                        LastRunDate = lastRunDate
                    };
                }
            }
            catch (Exception ex)
            {
                return dictionary;
            }

            return dictionary;
        }
    }
}
