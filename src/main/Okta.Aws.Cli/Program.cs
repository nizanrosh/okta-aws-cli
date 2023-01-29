using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Okta.Aws.Cli;
using Okta.Aws.Cli.Aws.ArnMappings;
using Okta.Aws.Cli.Aws.Profiles;
using Okta.Aws.Cli.Cli.Interfaces;
using Okta.Aws.Cli.Extensions;
using Okta.Aws.Cli.GitHub;
using Okta.Aws.Cli.Okta;
using Okta.Aws.Cli.Okta.Abstractions;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    cts.Cancel();
    e.Cancel = true;
};

var configuration = new ConfigurationBuilder()
    //.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false)
    .AddEnvironmentVariables()
    .ConfigureArnMappings()
    .ConfigureUserSettings()
    .ConfigureProfiles()
    .Build();

var services = new ServiceCollection();

services.AddLogging(builder => builder.ConfigureOktaAwsCliLogging());
services.AddSingleton<IConfiguration>(configuration);

services.AddOktaSamlProvider();
services.AddAwsCredentialsProvider();
services.AddFileSystemUpdaters();
services.AddCliArgumentHandling();

services.AddSingleton<IUserSettingsHandler, UserSettingsHandler>();
services.AddSingleton<IArnMappingsService, ArnMappingsService>();
services.AddSingleton<IProfilesService, ProfilesService>();
services.AddSingleton<IOktaAwsAssumeRoleService, OktaAwsAssumeRoleService>();

services.AddSingleton<OktaHttpClientHandler>();
services.AddHttpClient<IOktaApiClient, OktaApiClient>().ConfigurePrimaryHttpMessageHandler<OktaHttpClientHandler>();
services.AddHttpClient<IGitHubApiClient, GitHubApiClient>();

services.AddSingleton<IVersionService, VersionService>();

var provider = services.BuildServiceProvider();
var logger = provider.GetRequiredService<ILogger<Program>>();

try
{
    var relevantArgs = args.Where(arg => arg != "--debug").ToArray();

    var argumentFactory = provider.GetRequiredService<ICliArgumentFactory>();
    var appTask = argumentFactory.GetHandler(args.ElementAtOrDefault(0)).Handle(relevantArgs, cts.Token);

    var versionService = provider.GetRequiredService<IVersionService>();
    var versionTask =  versionService.ExecuteAsync(cts.Token);

    await Task.WhenAll(appTask, versionTask);
}
catch (Exception e)
{
    logger.LogError(e, "An error has occurred");
}