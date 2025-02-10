namespace FeedCord.Common
{
    public record Post(
        string Title,
        string ImageUrl,
        string Description,
        string Link,
        string Tag,
        DateTime PublishDate,
        string Author
        );
}
