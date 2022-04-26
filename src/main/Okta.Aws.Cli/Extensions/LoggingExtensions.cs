using Microsoft.Extensions.Logging;

namespace Okta.Aws.Cli.Extensions;

public static class LoggingExtensions
{
    public static ILoggingBuilder ConfigureOktaAwsCliLogging(this ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.ClearProviders();
        loggingBuilder
            .AddFilter("Microsoft", LogLevel.Error)
                .AddFilter("Default", LogLevel.Information)
                .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                .AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                });

        return loggingBuilder;
    }
}