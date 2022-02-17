using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Okta.Aws.Cli.Abstractions;
using Okta.Aws.Cli.Constants;
using Okta.Aws.Cli.Okta.Abstractions;
using Okta.Aws.Cli.Okta.Constants;

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

        var sessionId = await GetSessionId(sessionToken, userSettings.OktaDomain!, cancellationToken);
        var appUrl = string.IsNullOrEmpty(userSettings.AppUrl) ? await GetAppUrl(sessionId, cancellationToken) : userSettings.AppUrl;
        var html = await GetHtml(sessionId, userSettings.OktaDomain!, appUrl, cancellationToken);

        return html;
    }

    private async Task<string> GetSessionId(string sessionToken, string oktaDomain, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting session id...");

        var sessionRequest = new SessionRequest(sessionToken);
        var payload = JsonConvert.SerializeObject(sessionRequest);

        var sessionResponse = await _httpClient.PostAsJsonAsync($"{oktaDomain}/api/v1/sessions", sessionRequest, cancellationToken: cancellationToken);

        var sessionContent = await sessionResponse.Content.ReadAsStringAsync(cancellationToken);
        var sessionModel = JsonConvert.DeserializeObject<SessionResponse>(sessionContent);

        return sessionModel.Id;
    }

    private async Task<string> GetAppUrl(string sessionId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting aws app url...");

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{_configuration[User.Settings.OktaDomain]}/api/v1/users/me/appLinks");
        httpRequest.Headers.Add("Cookie", $"sid={sessionId}");

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var appLinks = JsonConvert.DeserializeObject<AppLinks[]>(content);

        return appLinks?.FirstOrDefault(al => al.AppName == AppNames.Amazon)?.LinkUrl;
    }

    private async Task<string> GetHtml(string sessionId, string oktaDomain, string appUrl, CancellationToken cancellationToken)
    {
        _httpClientHandler.CookieContainer.Add(new Uri(oktaDomain), new Cookie("sid", sessionId));

        var awsResponse = await _httpClient.GetAsync(appUrl, cancellationToken);

        var html = await awsResponse.Content.ReadAsStringAsync(cancellationToken);
        return html;
    }
}