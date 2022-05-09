using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Okta.Aws.Cli;
using Okta.Aws.Cli.Extensions;
using Okta.Aws.Cli.GitHub;
using Okta.Aws.Cli.Okta;
using Okta.Aws.Cli.Okta.Abstractions;

await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder => builder.ConfigureUserSettings())
    .ConfigureLogging(builder => builder.ConfigureOktaAwsCliLogging())
    .ConfigureServices(services =>
    {
        services.AddOktaSamlProvider();
        services.AddAwsCredentialsProvider();
        services.AddFileSystemUpdaters();
        services.AddCliArgumentHandling();

        services.AddSingleton<IUserSettingsHandler, UserSettingsHandler>();
        services.AddSingleton<IOktaAwsAssumeRoleService, OktaAwsAssumeRoleService>();

        services.AddSingleton<OktaHttpClientHandler>();
        services.AddHttpClient<IOktaApiClient, OktaApiClient>().ConfigurePrimaryHttpMessageHandler<OktaHttpClientHandler>();
        services.AddHttpClient<IGitHubApiClient, GitHubApiClient>();

        services.AddHostedService<VersionHostedService>();
        services.AddHostedService<MainAppHostedService>();
    })
    .RunConsoleAsync();