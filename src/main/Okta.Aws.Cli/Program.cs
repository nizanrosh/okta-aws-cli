using Abstractions;
using Amazon.Runtime.SharedInterfaces;
using Cocona;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Okta.Aws.Cli;
using Okta.Aws.Cli.Aws.ArnMappings;
using Okta.Aws.Cli.Aws.Profiles;
using Okta.Aws.Cli.Extensions;
using Okta.Aws.Cli.GitHub;
using Okta.Aws.Cli.Okta;
using Okta.Aws.Cli.Okta.Abstractions;
using Okta.Aws.Cli.Okta.Abstractions.Interfaces;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    cts.Cancel();
    e.Cancel = true;
};

var builder = CoconaApp.CreateBuilder(args);

var appSettingsPath = $"{AppPathResolver.GetAppPath()}/appsettings.json";
builder.Host.ConfigureAppConfiguration((_, configBuilder) =>
{
    configBuilder.AddJsonFile(appSettingsPath, false)
        .AddEnvironmentVariables()
        .ConfigureArnMappings()
        .ConfigureUserSettings()
        .ConfigureProfiles()
        .Build();
});

builder.Services.AddLogging(loggingBuilder => loggingBuilder.ConfigureOktaAwsCliLogging());

builder.Services.AddOktaSamlProvider();
builder.Services.AddAwsCredentialsProvider();
builder.Services.AddFileSystemUpdaters();
builder.Services.AddCliArgumentHandling();

builder.Services.AddSingleton<IUserSettingsHandler, UserSettingsHandler>();
builder.Services.AddSingleton<IArnMappingsService, ArnMappingsService>();
builder.Services.AddSingleton<IProfilesService, ProfilesService>();
builder.Services.AddSingleton<IOktaAwsAssumeRoleService, OktaAwsAssumeRoleService>();

builder.Services.AddHttpClient<IOktaSessionHttpClient, OktaSessionHttpClient>();

builder.Services.AddSingleton<OktaHttpClientHandler>();
builder.Services.AddHttpClient<IOktaApiHttpClient, OktaApiHttpClient>()
    .ConfigurePrimaryHttpMessageHandler<OktaHttpClientHandler>();
builder.Services.AddHttpClient<IGitHubApiClient, GitHubApiClient>();

builder.Services.AddSingleton<IVersionService, VersionService>();

var provider = builder.Services.BuildServiceProvider();

var app = builder.Build();


// app.AddCommand("run",
//     [IgnoreUnknownOptions] async (IRunArgumentHandler handler) =>
//     {
//         await handler.Handle(cts.Token);
//     });

app.AddRunCommand(provider, cts.Token)
    .AddConfigureCommand(cts.Token)
    .AddLogoutCommand(cts.Token)
    .AddResetCommand(cts.Token)
    .AddSelectCommand(provider, cts.Token)
    .AddWhoAmICommand(cts.Token);

await app.RunAsync();

// try
// {
//     var relevantArgs = args.Where(arg => arg != "--debug").ToArray();
//
//     var argumentFactory = provider.GetRequiredService<ICliArgumentFactory>();
//     var appTask = argumentFactory.GetHandler(args.ElementAtOrDefault(0)).Handle(relevantArgs, cts.Token);
//
//     var versionService = provider.GetRequiredService<IVersionService>();
//     var versionTask =  versionService.ExecuteAsync(cts.Token);
//
//     await Task.WhenAll(appTask, versionTask);
// }
// catch (Exception e)
// {
//     logger.LogError(e, "An error has occurred");
// }