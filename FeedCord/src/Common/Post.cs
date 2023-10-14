

namespace FeedCord.src.Common
{
    internal record Post(
        string Title,
        string ImageUrl,
        string Description,
        string Link,
        string Tag,
        DateTime PublishDate
        );
}
