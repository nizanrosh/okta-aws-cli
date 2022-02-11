using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Okta.Aws.Cli;
using Okta.Aws.Cli.Extensions;

await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder => builder.ConfigureUserSettings())
    .ConfigureLogging(builder =>
    {
        builder.ClearProviders();
        builder
            .AddFilter("Microsoft", LogLevel.Error)
            .AddFilter("Default", LogLevel.Information)
            .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
            .AddSimpleConsole(options =>
            {
                options.SingleLine = true;
            });
    })
    .ConfigureServices(services =>
    {
        services.AddOktaSamlProvider();
        services.AddAwsCredentialsProvider();
        services.AddFileCredentialsUpdater();
        services.AddCliArgumentHandling();

        services.AddSingleton<IUserSettingsHandler, UserSettingsHandler>();
        services.AddSingleton<IOktaAwsAssumeRoleService, OktaAwsAssumeRoleService>();

        services.AddHostedService<HostedService>();
    })
    .RunConsoleAsync();