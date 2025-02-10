
## appsettings.json reference

Your `appsettings.json` is a collection of `instances`:

```
{
    "Instances": []
}
```
Each Discord Channel is considered one instance, and gets one Webhook:

```
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
}
```

Here is an example of running two news channels:

```
{
	"Instances": [
		{
			"Id": "Gaming News Channel",
			"Username": "Gaming News",
			"RssUrls": [
				"https://examplesrssfeed1.com/rss",
				"https://examplesrssfeed2.com/rss",
				"https://examplesrssfeed3.com/rss"
			],
			"YoutubeUrls": [ "" ],
			"DiscordWebhookUrl": "https://discord.com/api/webhooks/...",
			"RssCheckIntervalMinutes": 3,
			"EnableAutoRemove": true,
			"Color": 8411391,
			"DescriptionLimit": 200,
			"Forum": true,
			"MarkdownFormat": false,
			"PersistenceOnShutdown": false
		},
		{
			"Id": "Tech News Channel",
			"Username": "Tech News",
			"RssUrls": [
				"https://examplesrssfeed4.com/rss",
				"https://examplesrssfeed5.com/rss",
				"https://examplesrssfeed6.com/rss"
			],
			"YoutubeUrls": [ "" ],
			"DiscordWebhookUrl": "https://discord.com/api/webhooks/...",
			"RssCheckIntervalMinutes": 3,
			"EnableAutoRemove": true,
			"Color": 8411391,
			"DescriptionLimit": 200,
			"Forum": true,
			"MarkdownFormat": false,
			"PersistenceOnShutdown": false
		}
	],
	
}

```

Some properties are required while others are optional. A bare-bones `appsettings.json` file would look like this:

```
{
	"Instances": [
		{
			"Id": "Runescape Feed",
			"YoutubeUrls": [
				""
			],
			"RssUrls": [
				"https://github.com/qolors/FeedCord/releases.atom"
			],
			"DiscordWebhookUrl": "https://discord.com/api/webhooks/...",
			"RssCheckIntervalMinutes": 15,
			"Color": 8411391,
			"DescriptionLimit": 500,
			"Forum": true,
			"MarkdownFormat": true,
			"PersistenceOnShutdown": true
		}
	],
	"ConcurrentRequests": 40
}
```

---

## Property References

### Required

- **Id**: The unique name of the RSS Feed Service. Helpful for logging purposes.
- **RssUrls**: The list of RSS Feeds you want to get posts from.
- **YoutubeUrls**: The list of RSS Feeds you want to get posts from. You need **at least 1 Url** here.
- **DiscordWebhookUrl**: The created Webhook from your designated Discord Text Channel.
- **RssCheckIntervalMinutes**: How often you want to check for new Posts from all of your Url feeds in minutes.
- **Color**: Color of the Post's embedded message.
- **DescriptionLimit**: Limits the length of the description of the post to this number.
- **Forum**: Determines if the post will be sent to a Forum type channel.
- **MarkdownFormat**: If set true, will post feed item in markdown instead of an Embed
- **PersistenceOnShutdown**: If set true, will store the last run date when restart or shutdown to catch missed posts

### Optional


- **Username**: The name of the bot that will be sending the messages.
- **EnableAutoRemove**: If set to true - FeedCord will kick a url out of the list after 3 failed attempts to parse the content.
- **AvatarUrl**: The displayed icon for the bot.
- **AuthorIcon**: The icon displayed for the Author.
- **AuthorName**: Display name of Author.
- **AuthorUrl**: The external link it will send users to when they click on the Author's Name.
- **FallbackImage**: FeedCord always attempts to grab the webpage's image from metadata. If for some reason this fails, it will display this image instead.

---

