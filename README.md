

---

# FeedCord: Self-hosted RSS Reader for Discord

FeedCord is a dead-simple RSS Reader designed to integrate seamlessly with Discord. With just a few configuration steps, you can have a news feed text channel up and running in your server.

---

## Features

- **Discord Integration:** Directly send your RSS feed updates to a Discord channel via a webhook.
- **Ease of Setup:** Configuration is a breeze with a simple JSON file. Just add your Webhook URL & RSS Feeds
- **Docker Support:** Deploying with Docker makes your life easier and is highly recommended.

---

## Quick Setup

### 1. Creating a Discord Webhook

![Discord Webhook](https://github.com/Qolors/FeedCord/blob/master/FeedCord/docs/images/webhooks.png)

---

### 2. Setting Up FeedCord

**Step 1:** Create a `FeedCord` folder at your desired location.

**Step 2:** Inside the `FeedCord` folder, create a configuration file named `appsettings.json` with the following content:

```json
{
  "RssUrls": [
    "YOUR",
    "RSS URLS",
    "HERE"
  ],
  "DiscordWebhookUrl": "YOUR_WEBHOOK_URL_HERE",
  "RssCheckIntervalMinutes": 10,
  "Username": "FeedCord",
  "AvatarUrl": "https://i.imgur.com/1asmEAA.png",
  "AuthorIcon": "https://i.imgur.com/1asmEAA.png",
  "AuthorName": "FeedCord",
  "AuthorUrl": "https://github.com/Qolors/FeedCord",
  "FallbackImage": "https://i.imgur.com/f8M2Y5s.png",
  "FooterImage": "https://i.imgur.com/f8M2Y5s.png",
  "Color": 16744576
}
```

Make sure to replace placeholders (e.g., `YOUR RSS URLS HERE`, `YOUR_WEBHOOK_URL_HERE`) with your actual data.
You can see what each property does [here](https://github.com/Qolors/FeedCord/blob/master/FeedCord/docs/reference.md).

---

### 3. Docker Deployment

**Step 1:** In the `FeedCord` folder, create a Docker Compose file named `docker-compose.yml`:

```yaml
version: "3.9"

services:
  myfeedcord:
    image: qolors/feedcord:latest
    container_name: FeedCord
    restart: unless-stopped
    volumes:
      - ./PATH/TO/MY/JSON/FILE/appsettings.json:/app/config/appsettings.json
```

Replace `./PATH/TO/MY/JSON/FILE/` with the actual path to your `appsettings.json`.

**Step 2:** Navigate to your `FeedCord` directory in your terminal and run:

```
docker-compose up -d
```

This will pull the latest FeedCord image from Docker Hub and start the service.

---

## Done!

With the above steps completed, FeedCord should now be running and posting updates from your RSS feeds directly to your Discord channel.

---

For more information and updates, visit the [FeedCord GitHub repository](https://github.com/Qolors/FeedCord).

---
