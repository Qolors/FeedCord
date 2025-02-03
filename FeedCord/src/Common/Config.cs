﻿using System.ComponentModel.DataAnnotations;

namespace FeedCord.src.Common
{
    public class Config
    {
        [Required(ErrorMessage = "The 'Id' Property is required. \"Id\": \"MyFeedName\"")]
        public string Id { get; set; }

        [Required(ErrorMessage = "RssUrls Property is required (use an empty array if none) - \"RssUrls\": [\"\"]")]
        public string[] RssUrls { get; set; }

        [Required(ErrorMessage = "YoutubeUrls Property is required (use an empty array if none) - \"YoutubeUrls\": [\"\"]")]
        public string[] YoutubeUrls { get; set; }

        [Required(ErrorMessage = "DiscordWebhookUrl Property is required.")]
        public string DiscordWebhookUrl { get; set; }

        [Required(ErrorMessage = "RssCheckIntervalMinutes Property is required.")]
        public int RssCheckIntervalMinutes { get; set; }
        public string? Username { get; set; }
        public string? AvatarUrl { get; set; }
        public string? AuthorIcon { get; set; }
        public string? AuthorName { get; set; }
        public string? AuthorUrl { get; set; }
        public string? FallbackImage { get; set; }
        public string? FooterImage { get; set; }
        public int Color { get; set; }

        

        public bool EnableAutoRemove { get; set; }

        [Required(ErrorMessage = "Description Limit Property is required.")]
        public int DescriptionLimit { get; set; }
        [Required(ErrorMessage = "Forum Property is required (True for Forum Channels, False for Text Channels)")]
        public bool Forum { get; set; }
    }
}
