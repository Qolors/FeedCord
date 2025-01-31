using FeedCord.src.Common;
using FeedCord.src.Helpers;
using FeedCord.src.Core;
using FeedCord.src.Services;
using FeedCord.src.Core.Interfaces;
using FeedCord.src.Core.Factories;
using FeedCord.src.Infrastructure.Http;
using FeedCord.src.Services.Factories;
using FeedCord.src.Infrastructure.Factories;
using FeedCord.src.Services.Interfaces;
using FeedCord.src.Infrastructure.Parsers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.ComponentModel.DataAnnotations;

namespace FeedCord.src
{
    public class Startup
    {
        public static void Initialize(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            host.Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(SetupConfiguration)
                .ConfigureLogging(SetupLogging)
                .ConfigureServices(SetupServices);
        }

        private static void SetupConfiguration(HostBuilderContext ctx, IConfigurationBuilder builder)
        {
            builder
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("config/appsettings.json", optional: false, reloadOnChange: true);
        }

        private static void SetupLogging(HostBuilderContext ctx, ILoggingBuilder logging)
        {
            logging.ClearProviders();
            logging.AddConsole(options => { options.FormatterName = "customlogsformatter"; })
                   .AddConsoleFormatter<CustomLogsFormatter, ConsoleFormatterOptions>();

            logging.AddFilter("Microsoft", LogLevel.Information);
            logging.AddFilter("Microsoft.Hosting", LogLevel.Warning);
            logging.AddFilter("System", LogLevel.Information);
            logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
        }

        private static void SetupServices(HostBuilderContext ctx, IServiceCollection services)
        {
            services.AddHttpClient("Default", httpClient =>
            {
                httpClient.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddTransient<ICustomHttpClient, CustomHttpClient>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient("Default");
                var logger = sp.GetRequiredService<ILogger<CustomHttpClient>>();

                return new CustomHttpClient(httpClient, logger);
            });

            services.AddTransient<IFeedWorkerFactory, FeedWorkerFactory>();
            services.AddTransient<IFeedManagerFactory, FeedManagerFactory>();
            services.AddTransient<INotifierFactory, NotifierFactory>();
            services.AddTransient<IDiscordPayloadServiceFactory, DiscordPayloadServiceFactory>();
            services.AddTransient<IRssParsingService, RssParsingService>();
            services.AddTransient<IImageParserService, ImageParserService>();
            services.AddTransient<IYoutubeParsingService, YoutubeParsingService>();
            services.AddTransient<IDiscordPayloadService, DiscordPayloadService>();

            var configs = ctx.Configuration.GetSection("Instances")
                .Get<List<Config>>() ?? new List<Config>();

            Console.WriteLine($"Number of configurations loaded: {configs.Count}");

            foreach (var c in configs)
            {
                Console.WriteLine($"Validating & Registering Background Service {c.Id}");

                ValidateConfiguration(c);

                services.AddSingleton<IHostedService>(sp =>
                {
                    var feedManagerFactory = sp.GetRequiredService<IFeedManagerFactory>();
                    var feedWorkerFactory = sp.GetRequiredService<IFeedWorkerFactory>();
                    var notifierFactory = sp.GetRequiredService<INotifierFactory>();
                    var discordPayloadServiceFactory = sp.GetRequiredService<IDiscordPayloadServiceFactory>();

                    var feedManager = feedManagerFactory.Create(c);
                    var discordPayloadService = discordPayloadServiceFactory.Create(c);
                    var notifier = notifierFactory.Create(c, discordPayloadService);

                    return feedWorkerFactory.Create(c, feedManager, notifier);
                });
            }
        }

        private static void ValidateConfiguration(Config config)
        {
            var context = new ValidationContext(config, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateObject(config, context, results, validateAllProperties: true))
            {
                var errors = string.Join("\n", results.Select(r => r.ErrorMessage));
                throw new InvalidOperationException($"Invalid config entry: {errors}");
            }
        }
    }
}


