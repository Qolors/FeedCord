

namespace FeedCord.Common
{
    public class FeedState
    {
        public bool IsYoutube { get; init; }
        public DateTime LastPublishDate { get; set; }
        public int ErrorCount { get; set; }
    }
}
