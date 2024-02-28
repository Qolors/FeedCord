using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Abstractions;

namespace FeedCord.src.Helpers
{
    public class CustomLogsFormatter : ConsoleFormatter
    {
        public CustomLogsFormatter() : base("customlogsformatter") { }

        public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
        {
            var originalColor = Console.ForegroundColor;
            var logLevel = logEntry.LogLevel;
            var message = logEntry.Formatter(logEntry.State, logEntry.Exception);

            Console.ForegroundColor = logLevel switch
            {
                LogLevel.Trace => ConsoleColor.Gray,
                LogLevel.Debug => ConsoleColor.Blue,
                LogLevel.Information => ConsoleColor.Green,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Critical => ConsoleColor.DarkRed,
                _ => ConsoleColor.White,
            };
            textWriter.WriteLine($"{logLevel.ToString()[0]}: {message}");
            Console.ForegroundColor = originalColor;
        }
    }

}
