using System.Net;
using System.Net.Http.Json;
using System.Security;
using Kurukuru;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Okta.Aws.Cli.Abstractions;
using Okta.Aws.Cli.Constants;
using Okta.Aws.Cli.Okta.Abstractions;
using Okta.Aws.Cli.Okta.Constants;
using Sharprompt;

namespace Okta.Aws.Cli.Okta;

public class OktaApiClient : IOktaApiClient
{
    private readonly ILogger<OktaApiClient> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly OktaHttpClientHandler _httpClientHandler;

    public OktaApiClient(ILogger<OktaApiClient> logger, IConfiguration configuration, HttpClient httpClient, OktaHttpClientHandler httpClientHandler)
    {
        _logger = logger;
        _configuration = configuration;

        _httpClient = httpClient;
        _httpClientHandler = httpClientHandler;
    }

    public async Task<string> GetSamlHtml(string sessionToken, CancellationToken cancellationToken)
    {
        var userSettings = _configuration.GetSection(nameof(UserSettings)).Get<UserSettings>();

        var sessionId = await LogIn(sessionToken, cancellationToken);
        var appUrl = IsAppUrlValid(userSettings.AppUrl) ? userSettings.AppUrl : await GetAppUrl(sessionId, cancellationToken);
        ArgumentNullException.ThrowIfNull(appUrl, nameof(appUrl));

        var html = await GetHtml(sessionId, userSettings.OktaDomain!, appUrl, cancellationToken);

        return html;
    }

    private async Task<string> LogIn(string sessionToken, CancellationToken cancellationToken)
    {
        var spinner = new Spinner("Logging in...");
        spinner.SymbolSucceed = new SymbolDefinition("V", "V");

        try
        {
            spinner.Start();

            var userSettings = _configuration.GetSection(nameof(UserSettings)).Get<UserSettings>();

            var sessionId = await GetSessionId(sessionToken, userSettings.OktaDomain!, cancellationToken);
            
            spinner.Succeed();

            return sessionId;
        }
        catch (Exception)
        {
            spinner.Fail();
            throw;
        }
    }

    private async Task<string> GetSessionId(string sessionToken, string oktaDomain, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting session id...");

        var sessionRequest = new SessionRequest(sessionToken);

        var sessionResponse = await _httpClient.PostAsJsonAsync($"{oktaDomain}/api/v1/sessions", sessionRequest, cancellationToken);

        var sessionContent = await sessionResponse.Content.ReadAsStringAsync(cancellationToken);
        var sessionModel = JsonConvert.DeserializeObject<SessionResponse>(sessionContent);
        ArgumentNullException.ThrowIfNull(sessionModel.Id, nameof(sessionModel.Id));

        return sessionModel.Id;
    }

    private async Task<string?> GetAppUrl(string sessionId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting aws app url...");

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{_configuration[User.Settings.OktaDomain]}/api/v1/users/me/appLinks");
        httpRequest.Headers.Add("Cookie", $"sid={sessionId}");

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var appLinks = JsonConvert.DeserializeObject<AppLink[]>(content);

        var appUrls = appLinks.Where(al => al.AppName == AppNames.Amazon).ToArray();
        if (!appUrls.Any()) return null;

        if (appUrls.Length == 1) return appUrls.First().LinkUrl;

        Prompt.ColorSchema.Select = ConsoleColor.Yellow;
        var selection = Prompt.Select("Select app url:", appUrls, textSelector: al => $"{al.Label} ({al.LinkUrl})");
        return selection.LinkUrl;
    }

    private async Task<string> GetHtml(string sessionId, string oktaDomain, string appUrl, CancellationToken cancellationToken)
    {
        _httpClientHandler.CookieContainer.Add(new Uri(oktaDomain), new Cookie("sid", sessionId));

        var awsResponse = await _httpClient.GetAsync(appUrl, cancellationToken);

        var html = await awsResponse.Content.ReadAsStringAsync(cancellationToken);
        return html;
    }

    private bool IsAppUrlValid(string? appUrl)
    {
        if (string.IsNullOrEmpty(appUrl)) return false;

        if (Uri.TryCreate(appUrl, UriKind.Absolute, out _) == false) return false;

        return true;
    }
}