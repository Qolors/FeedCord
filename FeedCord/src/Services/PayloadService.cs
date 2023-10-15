using FeedCord.src.Common;
using System.Text;
using System.Text.Json;

namespace FeedCord.src.Services
{
    internal static class PayloadService
    {
        public static StringContent BuildPayloadWithPost(Post post)
        {
            var payload = new
            {
                embeds = new[]
                {
                    new
                    {
                        title = post.Title,
                        author = new
                        {
                            name = "FeedCord News",
                            url = "https://github.com/Qolors/FeedCord",
                            icon_url = "https://i.imgur.com/1asmEAA.png"
                        },
                        url = post.Link,
                        description = post.Description,
                        image = new
                        {
                            url = string.IsNullOrEmpty(post.ImageUrl) ? "https://i.imgur.com/f8M2Y5s.png" : post.ImageUrl,
                        },
                        footer = new
                        {
                            text = $"{post.Tag} - {post.PublishDate.ToShortTimeString()}",
                        },
                    }
                }
            };

            var payloadJson = JsonSerializer.Serialize(payload);
            var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

            return content;
        }
    }
}
