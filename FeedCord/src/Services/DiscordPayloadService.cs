using FeedCord.src.Common;
using System.Text;
using System.Text.Json;

namespace FeedCord.src.Services
{
    internal static class DiscordPayloadService
    {
        private static Config _config;

        public static void SetConfig(Config config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public static StringContent BuildPayloadWithPost(Post post)
        {
            var payload = new
            {
                username = _config.Username,
                avatar_url = _config.AvatarUrl,
                embeds = new[]
                {
                    new
                    {
                        title = post.Title,
                        author = new
                        {
                            name = _config.AuthorName,
                            url = _config.AuthorUrl,
                            icon_url = _config.AuthorIcon
                        },
                        url = post.Link,
                        description = post.Description,
                        image = new
                        {
                            url = string.IsNullOrEmpty(post.ImageUrl) ? _config.FallbackImage : post.ImageUrl,
                        },
                        footer = new
                        {
                            text = $"{post.Tag} - {post.PublishDate:MM/dd/yyyy h:mm tt}",
                            icon_url = _config.FooterImage
                        },
                        color = _config.Color,
                    }
                }
            };

            var payloadJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return new StringContent(payloadJson, Encoding.UTF8, "application/json");
        }
    }
}
