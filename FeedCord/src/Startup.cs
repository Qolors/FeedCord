﻿using FeedCord.Common;
using FeedCord.Helpers;
using FeedCord.Core;
using FeedCord.Services;
using FeedCord.Core.Interfaces;
using FeedCord.Core.Factories;
using FeedCord.Infrastructure.Http;
using FeedCord.Services.Factories;
using FeedCord.Infrastructure.Factories;
using FeedCord.Services.Interfaces;
using FeedCord.Infrastructure.Parsers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.ComponentModel.DataAnnotations;

namespace FeedCord
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
                .ConfigureAppConfiguration((ctx, builder) =>
                {
                    SetupConfiguration(ctx, builder, args);
                })
                .ConfigureLogging(SetupLogging)
                .ConfigureServices(SetupServices);
        }

        private static void SetupConfiguration(HostBuilderContext ctx, IConfigurationBuilder builder, string[] args)
        {
            builder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
            builder.AddJsonFile(args.Length == 1 ? args[0] : "config/appsettings.json", optional: false,
                reloadOnChange: true);
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
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 " + 
                    "(KHTML, like Gecko) Chrome/104.0.5112.79 Safari/537.36"
                );
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler(){AllowAutoRedirect = true});

            var concurrentRequests = ctx.Configuration.GetValue("ConcurrentRequests", 20);

            if (concurrentRequests != 20)
            {
                Console.WriteLine($"Concurrent Requests set to: {concurrentRequests}");
            }

            services.AddSingleton(new SemaphoreSlim(concurrentRequests));
            
            services.AddSingleton<IBatchLogger, BatchLogger>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<BatchLogger>>();
                return new BatchLogger(logger);
            });

            services.AddTransient<ICustomHttpClient, CustomHttpClient>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient("Default");
                var logger = sp.GetRequiredService<ILogger<CustomHttpClient>>();
                var throttle = sp.GetRequiredService<SemaphoreSlim>();

                return new CustomHttpClient(logger, httpClient, throttle);
            });

            services.AddTransient<ILogAggregatorFactory, LogAggregatorFactory>();
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
                    var logAggregatorFactory = sp.GetRequiredService<ILogAggregatorFactory>();

                    var logAggregator = logAggregatorFactory.Create(c);
                    var feedManager = feedManagerFactory.Create(c, logAggregator);
                    var discordPayloadService = discordPayloadServiceFactory.Create(c);
                    var notifier = notifierFactory.Create(c, discordPayloadService);

                    return feedWorkerFactory.Create(c, logAggregator, feedManager, notifier);
                });
            }
        }

        private static void ValidateConfiguration(Config config)
        {
            var context = new ValidationContext(config, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();

            if (Validator.TryValidateObject(config, context, results, validateAllProperties: true)) 
                return;
            
            var errors = string.Join("\n", results.Select(r => r.ErrorMessage));
            throw new InvalidOperationException($"Invalid config entry: {errors}");
        }
    }
}


