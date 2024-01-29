using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;
using FeedCord.src.DiscordNotifier;
using FeedCord.src.Factories;
using FeedCord.src.Factories.Interfaces;
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
                    var configs = hostContext.Configuration.GetSection("Instances").Get<List<Config>>();

                    services
                    .AddSingleton<IRssCheckerBackgroundServiceFactory, RssCheckerBackgroundServiceFactory>()
                    .AddSingleton<IFeedProcessorFactory, FeedProcessorFactory>()
                    .AddSingleton<INotifierFactory, NotifierFactory>()
                    .AddScoped<IRssProcessorService, RssProcessorService>()
                    .AddTransient<IOpenGraphService, OpenGraphService>()
                    .AddTransient<IYoutubeParsingService, YoutubeParsingService>();

                    services.AddHttpClient("Default", httpClient =>
                    {
                        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                        httpClient.Timeout.Add(TimeSpan.FromSeconds(30));
                    });

                    foreach (var config in configs)
                    {

                        services.AddHostedService(sp =>
                        {
                            var feedProcessorFactory = sp.GetRequiredService<IFeedProcessorFactory>();
                            var rssCheckerBackgroundServiceFactory = sp.GetRequiredService<IRssCheckerBackgroundServiceFactory>();
                            var notifierFactory = sp.GetRequiredService<INotifierFactory>();

                            var feedProcessor = feedProcessorFactory.Create(config);
                            var notifier = notifierFactory.Create(config);

                            return rssCheckerBackgroundServiceFactory.Create(config, feedProcessor, notifier);
                        });
                        
                    }
                });


        private static void SetupConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
        {
            builder
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("config/appsettings.json", optional: false, reloadOnChange: true);
        }
    }
}


