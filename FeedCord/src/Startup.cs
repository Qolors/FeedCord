using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;
using FeedCord.src.DiscordNotifier;
using FeedCord.src.RssReader;
using FeedCord.src.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FeedCord.src
{
    public class Startup
    {
        public static void Initialize(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            host.Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(SetupConfiguration)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddFilter("Microsoft", LogLevel.Information);
                    logging.AddFilter("Microsoft.Hosting", LogLevel.Warning);
                    logging.AddFilter("System", LogLevel.Information);
                    logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var config = hostContext.Configuration;

                    var rssUrls = config.GetSection("RssUrls").Get<string[]>();
                    var discordWebhookUrl = config.GetValue<string>("DiscordWebhookUrl");
                    var username = config.GetValue<string>("Username");
                    var avatarUrl = config.GetValue<string>("AvatarUrl");
                    var authorIcon = config.GetValue<string>("AuthorIcon");
                    var authorName = config.GetValue<string>("AuthorName");
                    var authorUrl = config.GetValue<string>("AuthorUrl");
                    var fallbackImage = config.GetValue<string>("FallbackImage");
                    var footerImage = config.GetValue<string>("FooterImage");
                    var color = config.GetValue<int>("Color");
                    var rssCheckIntervalMinutes = config.GetValue<int>("RssCheckIntervalMinutes");

                    var appConfig = new Config(
                        rssUrls,
                        discordWebhookUrl,
                        username,
                        avatarUrl,
                        authorIcon,
                        authorName,
                        authorUrl,
                        fallbackImage,
                        footerImage,
                        color,
                        rssCheckIntervalMinutes);

                    services
                        .AddHttpClient()
                        .AddScoped<IRssProcessorService, RssProcessorService>()
                        .AddTransient<IOpenGraphService, OpenGraphService>()
                        .AddSingleton(appConfig)
                        .AddSingleton<IFeedProcessor>(serviceProvider =>
                        {
                            var config = serviceProvider.GetRequiredService<Config>();
                            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                            var rssService = serviceProvider.GetRequiredService<IRssProcessorService>();
                            var logger = serviceProvider.GetRequiredService<ILogger<FeedProcessor>>();

                            return FeedProcessor.CreateAsync(config, httpClientFactory, rssService, logger).GetAwaiter().GetResult();
                        })
                        .AddSingleton<INotifier, Notifier>()
                        .AddHostedService<RssCheckerBackgroundService>();
                });


        private static void SetupConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
        {
            builder
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("config/appsettings.json", optional: false, reloadOnChange: true);
        }
    }
}


