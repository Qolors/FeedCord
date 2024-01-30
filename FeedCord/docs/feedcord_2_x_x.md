# Release Summary: FeedCord 2.0.0

## What's New in 2.0.0

FeedCord 2.0.0 introduces exciting new features and some important changes to the configuration format. This major update brings the ability to run multiple webhook instances, adds support for Forum Type Channels on Discord, and updates several configuration properties to be optional.

### Features

1. **Multiple Webhook Instances**: You can now configure and run multiple instances of webhooks, allowing for greater flexibility and customization for different channels or servers.

2. **Support for Forum Type Channels**: FeedCord now supports [Forum Type Channels in Discord](https://discord.com/blog/forum-channels-space-for-organized-conversation). This can be configured easily using the new `Forum` property.

3. **Optional Configuration Properties**: To simplify setup and enhance flexibility, several configuration properties are now optional.

## Configuration Changes

### Updating Your `config.json` File

The format of `config.json` has been updated to support these new features. Here's what you need to know to update your configuration:

### New Format (v2.0.0 and above):
With 2.X.X, we now wrap our configurations in to an array like so:

```json
{
    "Instances": [
      { ..First Instance Properties.. },
      { ..Second Instance Properties.. },
      { ..Third Instance Properties.. }
    ]
}

```

With each Instance containing the following properties like before:

```json
{
  "Id": "First Instance Name",
  "RssUrls": [
    "YOUR",
    "RSS URLS",
    "HERE"
  ],
  "YoutubeUrls": [
    "YOUR",
    "YOUTUBE CHANNEL URLS",
    "HERE",
  ],
  "DiscordWebhookUrl": "https://discordapp.com/api/webhooks/...",
  "RssCheckIntervalMinutes": 3,
  "EnableAutoRemove": true,
  "Username": "FeedCord", <--------------------------------- * Now Optional
  "AvatarUrl": "https://i.imgur.com/1asmEAA.png", <--------- * Now Optional
  "AuthorIcon": "https://i.imgur.com/1asmEAA.png", <-------- * Now Optional
  "AuthorName": "FeedCord", <------------------------------- * Now Optional
  "AuthorUrl": "https://github.com/Qolors/FeedCord", <------ * Now Optional
  "FallbackImage": "https://i.imgur.com/f8M2Y5s.png", <----- * Now Optional
  "FooterImage": "https://i.imgur.com/f8M2Y5s.png", <------- * Now Optional
  "Color": 8411391,
  "DescriptionLimit": 200,
  "Forum": true
}
```

#### 2.X.X Format Example of running two Webhooks:


```json
{
  "Instances": [
    {
      "Id": "Gaming News Channel",
      "RssUrls": [
        "https://examplesrssfeed1.com/rss",
        "https://examplesrssfeed2.com/rss",
        "https://examplesrssfeed3.com/rss",

      ],
      "YoutubeUrls": [ "" ],
      "DiscordWebhookUrl": "https://discord.com/api/webhooks/...",
      "RssCheckIntervalMinutes": 3,
      "EnableAutoRemove": true,
      "Color": 8411391,
      "DescriptionLimit": 200,
      "Forum": true
    },
    {
      "Id": "Sports News Channel",
      "RssUrls": [
        "https://examplesrssfeed1.com/rss",
        "https://examplesrssfeed2.com/rss",
        "https://examplesrssfeed3.com/rss",
      ],
      "YoutubeUrls": [ "" ],
      "DiscordWebhookUrl": "https://discord.com/api/webhooks/...",
      "RssCheckIntervalMinutes": 3,
      "EnableAutoRemove": true,
      "Color": 8411391,
      "DescriptionLimit": 200,
      "Forum": false
    }
  ]  
}
```

### Key Changes to Note:

- The configuration is now wrapped inside an `"Instances"` array, allowing for multiple webhook configurations.
- The `Forum` property is introduced to specify if the webhook is for a Forum Type Channel (`true`) or a regular channel (`false`).
- Some properties have become optional to streamline the setup process.

## Migration Guide

To migrate your existing `config.json` file to the new format:

1. Wrap your current configuration object in an array and place it under the `"Instances"` key.
2. If you are using Forum Type Channels, add `"Forum": true` to each relevant instance in the array.
3. Make sure to include the `"Id"` property for each instance, as it is now required. This will be used to identify each instance in the logs and error messages.
4. Review your configuration to adjust or omit optional properties as needed.

Example migration for a single instance:

#### Before:

```json
{
  "RssUrls": ["https://example.com/feed"],
  "DiscordWebhookUrl": "https://discord.com/api/webhooks/12345",
  // Other properties...
}
```

#### After:

```json
{
  "Instances": [
    {
      "Id": "My Channel Feed", // REQUIRED
      "RssUrls": ["https://example.com/feed"],
      "DiscordWebhookUrl": "https://discord.com/api/webhooks/12345",
      "Forum": false/true, // REQUIRED
      // Other properties...
    }
  ]
}
```

## Closing Notes

If you have `Forum` set to `true` in your configuration, the channel must be created as a [Forum Type Channel in Discord](https://support.discord.com/hc/en-us/articles/6208479917079-Forum-Channels-FAQ#h_01G69FKE0ZAX9C65DCGMJGQKFE). If not, no posts will be made to the channel.

FeedCord 2.0.0 is a significant step forward, offering more versatility and better integration with Discord's evolving features. I appreciate the support and feedback from the community, and hope you enjoy the new features and improvements.

---
