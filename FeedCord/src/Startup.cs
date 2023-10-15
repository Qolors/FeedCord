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
        public async Task Initiliaze(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            host.Run();

            await Task.Delay(-1);
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddFilter("Microsoft", LogLevel.Information);
                    logging.AddFilter("System", LogLevel.Information);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration config = new ConfigurationBuilder()
                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                        .AddJsonFile("config/appsettings.json", optional: false, reloadOnChange: true)
                        .Build();

                    var rssUrls = config.GetSection("RssUrls").Get<string[]>()!;
                    var discordWebhookUrl = config.GetValue<string>("DiscordWebhookUrl")!;
                    int rssCheckIntervalMinutes = config.GetValue<int>("RssCheckIntervalMinutes");

                    services
                        .AddHttpClient()
                        .AddScoped<IRssProcessorService, RssProcessorService>()
                        .AddSingleton(new Config(rssUrls, discordWebhookUrl, rssCheckIntervalMinutes))
                        .AddSingleton<IFeedProcessor, FeedProcessor>()
                        .AddSingleton<INotifier, Notifier>()
                        .AddHostedService<RssCheckerBackgroundService>();
                });
    }
}
