using System.Net;
using Kurukuru;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Okta.Aws.Cli.Abstractions;
using Okta.Aws.Cli.Cli.Configurations;
using Okta.Aws.Cli.Constants;
using Okta.Aws.Cli.Okta.Abstractions;
using Okta.Aws.Cli.Okta.Abstractions.Interfaces;
using Okta.Aws.Cli.Okta.Constants;
using Sharprompt;

namespace Okta.Aws.Cli.Okta;

public class OktaApiHttpClient : IOktaApiHttpClient
{
    private readonly ILogger<OktaApiHttpClient> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly OktaHttpClientHandler _httpClientHandler;

    public OktaApiHttpClient(ILogger<OktaApiHttpClient> logger, IConfiguration configuration, HttpClient httpClient,
        OktaHttpClientHandler httpClientHandler)
    {
        _logger = logger;
        _configuration = configuration;

        _httpClient = httpClient;
        _httpClientHandler = httpClientHandler;
    }

    public async Task<SamlHtmlResponse> GetSamlHtml(RunConfiguration runConfiguration, string sessionId,
        CancellationToken cancellationToken)
    {
        var userSettings = _configuration.GetSection(nameof(UserSettings)).Get<UserSettings>();

        var spinner = new Spinner("Trying to get SAML...");
        spinner.Start();
        
        try
        {
            if (IsAppUrlValid(userSettings.AppUrl))
            {
                var validAppUrlSaml = await GetHtml(sessionId, userSettings.OktaDomain!, userSettings.AppUrl!,
                    cancellationToken);
                return new SamlHtmlResponse(validAppUrlSaml);
            }

            var appLinks = await GetMultipleAppLinks(sessionId, cancellationToken);
            ArgumentNullException.ThrowIfNull(appLinks, nameof(appLinks));

            if (appLinks.Length == 1)
            {
                var appLink = appLinks.First();
                var validAppUrlSaml = await GetHtml(sessionId, userSettings.OktaDomain!, appLink.LinkUrl,
                    cancellationToken);
                return new SamlHtmlResponse(validAppUrlSaml)
                {
                    SelectedAppUrl = new SelectedAppUrl(appLink.Label, appLink.LinkUrl)
                };
            }

            Prompt.ColorSchema.Select = ConsoleColor.Yellow;

            AppLink selection;

            if (ShouldUseDefaultAppUrl(runConfiguration, userSettings))
            {
                _logger.LogInformation(
                    $"Using saved account {runConfiguration.AppUrlAlias} ({runConfiguration.AppUrl})");
                spinner.Info($"Using saved app: {runConfiguration.AppUrlAlias} ({runConfiguration.AppUrl})");
                selection = new AppLink
                {
                    LinkUrl = runConfiguration.AppUrl,
                    Label = runConfiguration.AppUrlAlias
                };
            }
            else
            {
                spinner.Warn();
                selection = Prompt.Select("Select app url:", appLinks,
                    textSelector: al => $"{al.Label} ({al.LinkUrl})");

                spinner = new Spinner("Generating SAML...");
                spinner.Start();
            }

            var tasks = new List<Task<string>>
                { GetHtml(sessionId, userSettings.OktaDomain!, selection.LinkUrl, cancellationToken) };

            foreach (var appLink in appLinks.Where(al => al.LinkUrl != selection.LinkUrl))
            {
                tasks.Add(GetHtml(sessionId, userSettings.OktaDomain!, appLink.LinkUrl!, cancellationToken));
            }

            var results = await Task.WhenAll(tasks);

            spinner.Succeed();
            
            return new SamlHtmlResponse(results.First(), results[Range.StartAt(1)])
            {
                SelectedAppUrl = new SelectedAppUrl(selection.Label, selection.LinkUrl)
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error has occurred while creating SAML.");
            spinner.Fail();
            throw;
        }
    }

    private async Task<AppLink[]> GetMultipleAppLinks(string sessionId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting aws app url...");

        var httpRequest = new HttpRequestMessage(HttpMethod.Get,
            $"{_configuration[User.Settings.OktaDomain]}/api/v1/users/me/appLinks");
        httpRequest.Headers.Add("Cookie", $"sid={sessionId}");

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var appLinks = JsonConvert.DeserializeObject<AppLink[]>(content);

        var awsAppLinks = appLinks.Where(al => al.AppName == AppNames.Amazon).ToArray();
        if (!awsAppLinks.Any()) return null;

        return awsAppLinks;
    }

    private async Task<string> GetHtml(string sessionId, string oktaDomain, string appUrl,
        CancellationToken cancellationToken)
    {
        _httpClientHandler.CookieContainer.Add(new Uri(oktaDomain), new Cookie("sid", sessionId));

        var awsResponse = await _httpClient.GetAsync(appUrl, cancellationToken);

        var html = await awsResponse.Content.ReadAsStringAsync(cancellationToken);
        return html;
    }

    private bool IsAppUrlValid(string appUrl)
    {
        if (string.IsNullOrEmpty(appUrl)) return false;

        if (Uri.TryCreate(appUrl, UriKind.Absolute, out _) == false) return false;

        return true;
    }

    private bool ShouldUseDefaultAppUrl(RunConfiguration runConfiguration, UserSettings userSettings)
    {
        if (string.IsNullOrEmpty(runConfiguration.SubCommand) == false &&
            runConfiguration.SubCommand is Commands.Sub.Save or Commands.Sub.Fresh) return false;

        if (string.IsNullOrEmpty(userSettings.DefaultAwsAccount) ||
            string.IsNullOrEmpty(userSettings.DefaultAwsRole)) return false;

        return true;
    }
}

public class SamlHtmlResponse
{
    public string SelectedSaml { get; }
    public IReadOnlyCollection<string> AdditionalSamls { get; }

    public SelectedAppUrl SelectedAppUrl { get; set; }

    public SamlHtmlResponse(string selectedSaml, IReadOnlyCollection<string> additionalSamls = null)
    {
        SelectedSaml = selectedSaml;
        AdditionalSamls = additionalSamls;
    }
}

public class SelectedAppUrl
{
    public string Name { get; }
    public string AppUrl { get; }

    public SelectedAppUrl(string name, string appUrl)
    {
        Name = name;
        AppUrl = appUrl;
    }
}