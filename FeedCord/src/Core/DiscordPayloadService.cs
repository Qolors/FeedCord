using FeedCord.Common;
using FeedCord.Core.Interfaces;
using System.Text;
using System.Text.Json;
using System.Net.Http;

namespace FeedCord.Core
{
    public class DiscordPayloadService : IDiscordPayloadService
    {
        private Config _config;

        public DiscordPayloadService(Config config)
        {
            _config = config;
        }

        public StringContent BuildPayloadWithPost(Post post)
        {
            if (_config.MarkdownFormat)
                return GenerateMarkdown(post);
            
            var payload = new
            {
                username = _config.Username ?? "FeedCord",
                avatar_url = _config.AvatarUrl ?? "",
                embeds = new[]
                {
                    new
                    {
                        title = post.Title,
                        author = new
                        {
                            name = _config.AuthorName ?? post.Author,
                            url = _config.AuthorUrl ?? "",
                            icon_url = _config.AuthorIcon ?? ""
                        },
                        url = post.Link,
                        description = post.Description,
                        image = new
                        {
                            url = string.IsNullOrEmpty(post.ImageUrl) ? _config.FallbackImage ?? "" : post.ImageUrl,
                        },
                        footer = new
                        {
                            text = $"{post.Tag} - {post.PublishDate:MM/dd/yyyy h:mm tt}",
                            icon_url = _config.FooterImage ?? ""
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

        public StringContent BuildForumWithPost(Post post)
        {
            if (_config.MarkdownFormat)
                return GenerateMarkdown(post);
            
            var payload = new
            {
                content = post.Tag,
                embeds = new[]
                {
                    new
                    {
                        title = post.Title,
                        author = new
                        {
                            name = post.Author,
                            url = _config.AuthorUrl ?? "",
                            icon_url = _config.AuthorIcon ?? ""
                        },
                        url = post.Link,
                        description = post.Description,
                        image = new
                        {
                            url = string.IsNullOrEmpty(post.ImageUrl) ? _config.FallbackImage ?? "" : post.ImageUrl,
                        },
                        footer = new
                        {
                            text = $"{post.Tag} - {post.PublishDate:MM/dd/yyyy h:mm tt}",
                            icon_url = _config.FooterImage ?? ""
                        },
                        color = _config.Color,
                    }
                },
                thread_name = post.Title.Length > 100 ? post.Title[..99] : post.Title
            };

            var payloadJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return new StringContent(payloadJson, Encoding.UTF8, "application/json");
        }
        
        private StringContent GenerateMarkdown(Post post)
        {
            var markdownPost = $"""
                                # {post.Title}

                                > **Published**: {post.PublishDate:MMMM dd, yyyy}  
                                > **Author**: {post.Author}  
                                > **Feed**: {post.Tag}

                                {post.Description}

                                [Source]({post.Link})

                                """;
            object? payload = null;
            
            if (_config.Forum)
            {
                payload = new
                {
                    content = markdownPost,
                    thread_name = post.Title.Length > 100 ? 
                        post.Title[..99] : 
                        post.Title
                };
            }
            else
            {
                payload = new
                {
                    content = markdownPost
                };
            }
            
            var payloadJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            return new StringContent(payloadJson, Encoding.UTF8, "application/json");
        }
    }
}
