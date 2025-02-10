
![FeedCord Banner](https://github.com/Qolors/FeedCord/blob/master/FeedCord/docs/images/FeedCord.png)
---

# FeedCord: Self-hosted RSS Reader for Discord

FeedCord is designed to be a 'turn key' automated RSS feed reader with the main focus on Discord Servers. 

Use it for increasing community engagement and activity or just for your own personal use. The combination of FeedCord and Discord's Forum Channels can really shine to make a vibrant news feed featuring gallery-style display alongside custom threads, creating an engaging space for your private community discussions.

## An example of what FeedCord can bring to your server

![FeedCord Gallery 1](https://github.com/Qolors/FeedCord/blob/master/FeedCord/docs/images/gallery1.png)

![FeedCord Gallery 2](https://github.com/Qolors/FeedCord/blob/master/FeedCord/docs/images/gallery2.png)

A showing of one channel. Run as many of these as you want!

---
## Running FeedCord

FeedCord is very simple to get up and running. It only takes a few steps:

- Create a Discord Webhook
- Create and Edit a local file or two

Provided below is a quick guide to get up and running.


## Quick Setup

### 1. Create a new folder with a new file named `appsettings.json` inside with the following content:

```json
{
  "Instances": [
    {
      "Id": "My First News Feed",
      "YoutubeUrls": [
        ""
      ],
      "RssUrls": [
        ""
      ],
      "Forum": false,
      "DiscordWebhookUrl": "...",
      "RssCheckIntervalMinutes": 25,
      "EnableAutoRemove": false,
      "Color": 8411391,
      "DescriptionLimit": 250,
      "MarkdownFormat": false,
      "PersistenceOnShutdown": true
    }
  ],
  "ConcurrentRequests": 40
}
```
There is currently 17 properties you can configure. You can read more in depth explanation of the file structure as well as view all properties and their purpose [here](https://github.com/Qolors/FeedCord/blob/master/FeedCord/docs/reference.md)

---

### 2. Create a new Webhook in Discord (Visual Steps Provided)

![Discord Webhook](https://github.com/Qolors/FeedCord/blob/master/FeedCord/docs/images/webhooks.png)


### Quick Note

Be sure to populate your `appsettings.json` *"DiscordWebhookUrl"* property with your newly created Webhook

Before you actually run FeedCord, make sure you have populated your `appsettings.json` with RSS and YouTube feeds.

**RSS Feeds**

- For new users that aren't bringing their own list check out [awesome-rss-feeds](https://github.com/plenaryapp/awesome-rss-feeds) and add some that interest you
- Each url is entered by line seperating by comma. It should look like this in your `appsettings.json` file:

```json
"RssUrls": [
       "https://examplesrssfeed1.com/rss",
       "https://examplesrssfeed2.com/rss",
       "https://examplesrssfeed3.com/rss",
     ]
```

**YouTube Feeds**

- You can bring your favorite YouTube channels as well to be notified of new uploads
- FeedCord parses from the channel's base url so simply navigate to the channel home page and use that url.
- Example here if I was interested in Unbox Therapy & Tyler1:

```json
"YoutubeUrls": [
       "https://www.youtube.com/@unboxtherapy",
       "https://www.youtube.com/@TYLER1LOL"
     ]
```

### Running FeedCord

Now that your file is set up, you have two ways to run FeedCord

### Docker (Recommended)

```
docker pull qolors/feedcord:latest
```
Be sure to update the volume path to your `appsettings.json` 
```
docker run --name FeedCord -v "/path/to/your/appsettings.json:/app/config/appsettings.json"
```

### Build From Source

Install the [.NET SDK](dotnet.microsoft.com/download)

Clone this repo
```
git clone https://github.com/Qolors/FeedCord
```
Change Directory
```
cd FeedCord
```
Restore Dependencies
```
dotnet restore
```
Build
```
dotnet build
```
Run with your `appsettings.json` (provide your own path)
```
dotnet run -- path\to\your\appsettings.json
```


With the above steps completed, FeedCord should now be running and posting updates from your RSS feeds directly to your Discord channel.

<a href="https://www.buymeacoffee.com/Qolors" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/default-orange.png" alt="Buy Me A Coffee" height="41" width="174"></a>

---

# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

<details>
 <summary>[3.0.0] - 2025-02-10</summary>

### Added

- Restart persistence to catch up on missed posts if it had shutdown
- UserAgent cycling for failed get requests with retry attempts
- Multiple retry attempts on getting a post image
- Control over allowed concurrent HTTP requests FeedCord can make
- Separate handling of Reddit Feeds
- Markdown Support

### Changed

- README
- Large codebase refactoring

### Fixed

- Atom Feeds not returning a description
- Failed posting to Discord due to title length

</details>

<details>
 <summary>[2.1.1] - 2024-04-25</summary>

 ### Added

 - Added author being sourced from feed items
 - Added GZIP support for feeds
 
</details>


<details>
 <summary>[2.1.0] - 2024-02-28</summary>

 ### Added
 
 - Added Support for grabbing multiple new posts if the feed has multiple new posts since the last check.

 ### Changed
 
 - Improved Documentation for easier setup and understanding
 - Improved Logging for better readability
 - Posting now has a hard-coded 10 second buffer so large feeds respect Discord's rate limits

</details>


<details>
  <summary>[2.0.1] - 2024-02-19</summary>

  ### Added

  - Added Support for Reddit Feed & Better Atom Parsing Feeds

</details>

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
