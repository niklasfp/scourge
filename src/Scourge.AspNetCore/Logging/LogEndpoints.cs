using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Scourge.AspNetCore.Log;

namespace Scourge.AspNetCore.Logging;

internal abstract class Logger
{ };

internal static class LogEndpoints
{
    public static RouteGroupBuilder MapLogApi(this RouteGroupBuilder group)
    {
        group.WithTags("Logging");

        group.MapPost("{level?}", (ILogger<Logger> logger, [FromQuery] string message, [FromQuery] LogLevelType? level) =>
            {
                logger.Log(LogLevelTypeToLogLevel(level ?? LogLevelType.Information), message);
            })
            .WithOpenApi(operation =>
            {
                operation.Description = "Logs a message with the provided log level, information is default";
                operation.Summary = "Log messages.";
                return operation;
            });

        return group;
    }

    private static LogLevel LogLevelTypeToLogLevel(LogLevelType logType)
    {
        return logType switch
        {
            LogLevelType.Information => LogLevel.Information,
            LogLevelType.Error => LogLevel.Error,
            LogLevelType.Critical => LogLevel.Critical,
            LogLevelType.Warning => LogLevel.Warning,
            LogLevelType.Trace => LogLevel.Trace,
            LogLevelType.Debug => LogLevel.Debug,
            _ => throw new ArgumentOutOfRangeException(nameof(logType), logType, null)
        };
    }
}

