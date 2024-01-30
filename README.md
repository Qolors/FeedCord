
![FeedCord Banner](https://github.com/Qolors/FeedCord/blob/master/FeedCord/docs/images/FeedCord.png)
---

# FeedCord: Self-hosted RSS Reader for Discord

FeedCord is a dead-simple RSS Reader designed to integrate seamlessly with Discord. With just a few configuration steps, you can have a news feed text channel up and running in your server.

With the latest 2.0.0 release, FeedCord now supports multiple Webhook URLs, as well as support for Discord's Forum Channels.

If you are looking to transition to 2.0.0+ from 1.x.x, please see the [Migration Guide](https://github.com/Qolors/FeedCord/blob/master/FeedCord/docs/feedcord_2_x_x.md)

If you wish to show your support

<a href="https://www.buymeacoffee.com/Qolors" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/default-orange.png" alt="Buy Me A Coffee" height="41" width="174"></a>
---

## Features

- **Discord Integration:** Directly send your RSS feed updates to a Discord channel via a webhook.
- **Ease of Setup:** Configuration is a breeze with a simple JSON file. Just add your Webhook URL & RSS Feeds.
- **Docker Support:** Deploying with Docker makes your life easier and is highly recommended.

---

## Quick Setup (Docker)

### 1. Creating a Discord Webhook

![Discord Webhook](https://github.com/Qolors/FeedCord/blob/master/FeedCord/docs/images/webhooks.png)

---

### 2. Setting Up FeedCord

**Step 1:** Create a `FeedCord` folder at your desired location.

**Step 2:** Inside the `FeedCord` folder, create a configuration file named `appsettings.json` with the following content:

---
#### For the Latest Versions (2.0.0+) please refer to the [Guide](https://github.com/Qolors/FeedCord/blob/master/FeedCord/docs/feedcord_2_x_x.md)
---
#### For Version 1.3.0 and below

```json
{
  "RssUrls": [
    "YOUR",
    "RSS URLS",
    "HERE"
  ],
  "YoutubeUrls": [
    "YOUR",
    "YOUTUBE CHANNEL URLS",
    "HERE",
    "eg. https://www.youtube.com/@IGN"
  ],
  "DiscordWebhookUrl": "https://discordapp.com/api/webhooks/1139357708546478200/ncB3dshJOPkQhthwOFQibeNt6YI-1_DiFbg0B3ZecfxchnbCGQNdG-m3PxqDdDSvt5Kk",
  "RssCheckIntervalMinutes": 3,
  "EnableAutoRemove": true,
  "Username": "FeedCord",
  "AvatarUrl": "https://i.imgur.com/1asmEAA.png",
  "AuthorIcon": "https://i.imgur.com/1asmEAA.png",
  "AuthorName": "FeedCord",
  "AuthorUrl": "https://github.com/Qolors/FeedCord",
  "FallbackImage": "https://i.imgur.com/f8M2Y5s.png",
  "FooterImage": "https://i.imgur.com/f8M2Y5s.png",
  "Color": 8411391,
  "DescriptionLimit": 200
}
```
---
Make sure to replace placeholders (e.g., `YOUR RSS URLS HERE`, `YOUR_WEBHOOK_URL_HERE`) with your actual data.
You can see what each property does [here](https://github.com/Qolors/FeedCord/blob/master/FeedCord/docs/reference.md).

**Note:**: For a Youtube Channel FeedCord handles retreiving the RSS/XML Url for you. This means you just need to provide the Youtube Channel's home page url. This will look something like:
- 'https://www.youtube.com/@APopularChannel'
- 'https://www.youtube.com/channel/UCVfiai2'

---

### 3. Docker Deployment

**Step 1:** In the `FeedCord` folder, create a Docker Compose file named `docker-compose.yaml`:

```yaml
version: "3.9"

services:
  myfeedcord:
    image: qolors/feedcord:latest # for amd64 architecture
    # image: qolors/feedcord:latest-arm64  # For arm64 architecture (Uncomment this line and comment the above if using arm64)
    container_name: FeedCord
    restart: unless-stopped
    volumes:
      - ./PATH/TO/MY/JSON/FILE/appsettings.json:/app/config/appsettings.json
```

Replace `./PATH/TO/MY/JSON/FILE/` with the actual path to your `appsettings.json`.

**Note:** Depending on your architecture, use `qolors/feedcord:latest` for amd64 architecture, or `qolors/feedcord:latest-arm64` for arm64 architecture. Ensure to uncomment the appropriate line in the docker-compose.yml as per your system's architecture. If you need a different please open a request.

**Step 2:** Navigate to your `FeedCord` directory in your terminal and run:

```
docker-compose up -d
```

This will pull the latest FeedCord image from Docker Hub and start the service.

If you want to update your current image to latest it's quite simple. In your FeedCord directory run:
```
docker-compose pull
```
followed by
```
docker-compose up -d
```
This will pull the latest image and restart your current container with it
---

## Done

With the above steps completed, FeedCord should now be running and posting updates from your RSS feeds directly to your Discord channel.

---

# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

<details>
  <summary>[2.0.0] - 2024-01-30</summary>

  ### Added

  - Added Support for Multiple Webhook Urls & Configurations
  - Added Support for Discord's Forum Channels
  
  ### Changed

  - Configuration File formatting has changed to support multiple Webhook URLs
  - Slight improvements to Logging
  - Some Configuration properties are now optional rather than required

</details>


<details>
  <summary>[1.3.0] - 2024-01-20</summary>

  ### Added

  - Added Description Length Configuration

  ### Changed

  - Improved RSS & ATOM Parsing with implementing [FeedReader](https://github.com/arminreiter/FeedReader) library

  ### Fixed

  - RSS/ATOM Feeds returning errors because of parsing issues

</details>


<details>
  <summary>[1.2.1] - 2024-01-17</summary>

  ### Changed

  - Made Youtube URLs an optional addition rather than required

</details>

<details>
  <summary>[1.2.0] - 2023-10-25</summary>
  
  ### Added

  - Added Support for Youtube Channel Feeds in configuration file.
  - Added an optional Auto Remove option in configuration file for bad URL Feeds to get booted out of the list after multiple failed attempts.

  ### Changed

  - Improved container logging messages for better readability.

  ### Fixed

  - Color setting in configuration now properly works for the embed message
  - Fixed the handling of errors and removed from logging to reduce spam.
  - Fixed a known logging index error.

</details>

<details>
  <summary>[1.1.0] - 2023-10-16</summary>
  
  ### Added

  - Broke up `RssProcessorService` class to follow SOLID principles, adding a new service class `OpenGraphService` to handle meta tags.
  - Added `Helper` namespace & `StringHelper` class, which includes the `StripTags` method for potential reuse and improved organization.

  ### Changed

  - Enhanced the RSS feed background service for more efficient feed checks, reducing chances of delays.
  - Customized the `HttpClient` to set default request headers, ensuring better compatibility with certain RSS feeds.
  - Refined feed processing logic to include concurrent processing, beneficial for users with a large number of RSS feeds.
  - ReadMe to show this change log and multiple OS images.

  ### Fixed

  - Improved RSS feed initialization, ensuring only valid feeds are added to the tracking list.
  - Overhauled logs to not contain as much spam and allow for better readability.

</details>

<details>
  <summary>[1.0.0] - 2023-10-15</summary>
  
  ### Added
  - Initial Project Release

</details>


---
