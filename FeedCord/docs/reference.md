
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
			"MarkdownFormat": false,
			"PersistenceOnShutdown": true
		}
	],
	"ConcurrentRequests": 40
}
```
### Concurrent Requests

You have two `ConcurrentRequest` properties. One that lives outside the Instance array, and the other that lives for each instance.

Let's take a look here for example. This is outside of your instance. This controls the maximum amount of requests that can be made at the same time. This blankets over all instances. So a `ConcurrentRequests` value of 5 here says that no matter how may instances I have running, only allow 5 requests being made at any given time. This is to help throttle your whole application for control if needed:

```
{
	"Instances": [ ..All of My Instances.. ],
	"ConcurrentRequests": 5
}
```

Now for this example we will show a `ConcurrentRequest` inside of an instance. This will only throttle the YouTube & RSS Urls in that instance. So a `ConcurrentRequests` value of 1 here says that we only allow one request to be made at any given time for the `Gaming` instance. This is if you want to have control only for certain urls/domains. Useful if you need to respect a websites policy or to not spam a domain:

```
{
	"Instances": [
		{
			"Id": "Gaming",
			
			..My Other Properties..
			
			"ConcurrentRequests": 1
		}
	],
	"ConcurrentRequests": 40
}
```

### Post Filters

Post filters are useful if you are looking to sift out specific content from an RSS Feed. The `PostFilter` property is an array of objects. Each object has a `Url` & `Filters`. This allows you to set a specific filter for each url, but if you are looking to apply a general filter to all urls you can do that as well - see below:

```
{
	"Instances": [
		{
			"Id": "FeedCord",
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
			"MarkdownFormat": false,
			"PersistenceOnShutdown": true,
			"ConcurrentRequests": 10,
			"PostFilters": [
			{
				"Url": "https://github.com/qolors/FeedCord/releases.atom",
				"Filters": ["release", "new feature"]
			}
		]
		}
	],
	"ConcurrentRequests": 40
}
```

As you can see above we did the following:

- Assign our RssUrl a Filter by providing the direct url
- Assign two string values that must be contained in the posts content, otherwise skipped.

Here is an example of two urls with each their own filter:

```
{
	"Instances": [
		{
			"Id": "FeedCord",
			"YoutubeUrls": [
				""
			],
			"RssUrls": [
				"https://github.com/qolors/FeedCord/releases.atom",
				"https://github.com/qolors/Clam-Shell/releases.atom"
			],
			"DiscordWebhookUrl": "https://discord.com/api/webhooks/...",
			"RssCheckIntervalMinutes": 15,
			"Color": 8411391,
			"DescriptionLimit": 500,
			"Forum": true,
			"MarkdownFormat": false,
			"PersistenceOnShutdown": true,
			"ConcurrentRequests": 10,
			"PostFilters": [
			{
				"Url": "https://github.com/qolors/FeedCord/releases.atom",
				"Filters": ["release", "new feature"]
			},
			{
				"Url": "https://github.com/qolors/Clam-Shell/releases.atom",
				"Filters": ["phishing"]
			}
		]
		}
	],
	"ConcurrentRequests": 40
}
```
Great. But what if we have like 30 urls that we want to apply the same filter to? It could get quite tedious..

Luckily you can simply do this to do a filter for all feeds - set `Url` equal to `all`:

```
{
	"Instances": [
		{
			"Id": "FeedCord",
			"YoutubeUrls": [
				""
			],
			"RssUrls": [
				"https://github.com/qolors/FeedCord/releases.atom",
				"https://github.com/qolors/Clam-Shell/releases.atom"
			],
			"DiscordWebhookUrl": "https://discord.com/api/webhooks/...",
			"RssCheckIntervalMinutes": 15,
			"Color": 8411391,
			"DescriptionLimit": 500,
			"Forum": true,
			"MarkdownFormat": false,
			"PersistenceOnShutdown": true,
			"ConcurrentRequests": 10,
			"PostFilters": [
			{
				"Url": "all",
				"Filters": ["release", "new feature", "phishing"]
			}
		]
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
- **YoutubeUrls**: The list of RSS Feeds you want to get posts from.
- **DiscordWebhookUrl**: The created Webhook from your designated Discord Text Channel.
- **RssCheckIntervalMinutes**: How often you want to check for new Posts from all of your Url feeds in minutes.
- **Color**: Color of the Post's embedded message.
- **DescriptionLimit**: Limits the length of the description of the post to this number.
- **Forum**: Determines if the post will be sent to a Forum type channel.
- **MarkdownFormat**: If set true, will post feed item in markdown instead of an Embed.
- **PersistenceOnShutdown**: If set true, will store the last run date when restart or shutdown to catch missed posts.

### Optional


- **Username**: The name of the bot that will be sending the messages.
- **EnableAutoRemove**: If set to true - FeedCord will kick a url out of the list after 3 failed attempts to parse the content.
- **AvatarUrl**: The displayed icon for the bot.
- **AuthorIcon**: The icon displayed for the Author.
- **AuthorName**: Display name of Author.
- **AuthorUrl**: The external link it will send users to when they click on the Author's Name.
- **FallbackImage**: FeedCord always attempts to grab the webpage's image from metadata. If for some reason this fails, it will display this image instead.
- **ConcurrentRequests**: How many requests FeedCord can have going at once.
- **ConcurrentRequests (Inside Instance)**: How many requests the instance itself can have going at once.
- **PostFilters**: A collection of phrases/words that are used to filter out RSS Items (filters the Title & Content)

---

