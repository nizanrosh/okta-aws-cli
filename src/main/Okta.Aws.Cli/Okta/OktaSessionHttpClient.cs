using System.Net.Http.Json;
using Kurukuru;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Okta.Aws.Cli.Abstractions;
using Okta.Aws.Cli.Okta.Abstractions;
using Okta.Aws.Cli.Okta.Abstractions.Interfaces;

namespace Okta.Aws.Cli.Okta;

public class OktaSessionHttpClient : IOktaSessionHttpClient
{
    private readonly ILogger<OktaSessionHttpClient> _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public OktaSessionHttpClient(ILogger<OktaSessionHttpClient> logger, HttpClient httpClient, IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClient;
        _configuration = configuration;
    }
    
    public async Task<OktaSession> LogIn(string sessionToken, CancellationToken cancellationToken)
    {
        var spinner = new Spinner("Logging in...");
        //spinner.SymbolSucceed = new SymbolDefinition("V", "V");

        try
        {
            spinner.Start();

            var userSettings = _configuration.GetSection(nameof(UserSettings)).Get<UserSettings>();

            var session = await GetSessionId(sessionToken, userSettings.OktaDomain!, cancellationToken);

            spinner.Succeed();

            return session;
        }
        catch (Exception)
        {
            spinner.Fail();
            throw;
        }
    }

    private async Task<OktaSession> GetSessionId(string sessionToken, string oktaDomain, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting session id...");

        var sessionRequest = new SessionRequest(sessionToken);

        var sessionResponse =
            await _httpClient.PostAsJsonAsync($"{oktaDomain}/api/v1/sessions", sessionRequest, cancellationToken);

        var sessionModel = await sessionResponse.Content.ReadFromJsonAsync<OktaSession>(cancellationToken: cancellationToken);
        ArgumentNullException.ThrowIfNull(sessionModel.Id, nameof(sessionModel.Id));

        return sessionModel;
    }
}