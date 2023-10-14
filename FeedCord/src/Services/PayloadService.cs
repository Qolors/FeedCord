using FeedCord.src.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
                        url = post.Link,
                        description = post.Description,
                        image = new
                        {
                            url = post.ImageUrl,
                        },
                        footer = new
                        {
                            text = $"{post.Tag} - {post.PublishDate.ToShortTimeString()}",
                            icon_url = "https://cdn.discordapp.com/embed/avatars/index.png"
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
