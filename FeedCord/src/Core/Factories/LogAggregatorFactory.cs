using FeedCord.Common;
using FeedCord.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FeedCord.Core.Factories;

public class LogAggregatorFactory : ILogAggregatorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public LogAggregatorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ILogAggregator Create(Config config)
    {
        return ActivatorUtilities.CreateInstance<LogAggregator>(_serviceProvider, config);
    }
}