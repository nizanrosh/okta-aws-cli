using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Okta.Aws.Cli;
using Okta.Aws.Cli.Extensions;
using Okta.Aws.Cli.Okta;
using Okta.Aws.Cli.Okta.Abstractions;

await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder => builder.ConfigureUserSettings())
    .ConfigureLogging(builder => builder.ConfigureOktaAwsCliLogging())
    .ConfigureServices(services =>
    {
        services.AddOktaSamlProvider();
        services.AddAwsCredentialsProvider();
        services.AddFileCredentialsUpdater();
        services.AddCliArgumentHandling();

        services.AddSingleton<IUserSettingsHandler, UserSettingsHandler>();
        services.AddSingleton<IOktaAwsAssumeRoleService, OktaAwsAssumeRoleService>();

        services.AddHostedService<HostedService>();

        services.AddSingleton<OktaHttpClientHandler>();
        services.AddHttpClient<IOktaApiClient, OktaApiClient>().ConfigurePrimaryHttpMessageHandler<OktaHttpClientHandler>();
    })
    .RunConsoleAsync();