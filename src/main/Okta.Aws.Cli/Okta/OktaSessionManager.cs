using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Okta.Aws.Cli.Helpers;
using Okta.Aws.Cli.Okta.Abstractions;
using Okta.Aws.Cli.Okta.Abstractions.Interfaces;

namespace Okta.Aws.Cli.Okta;

public class OktaSessionManager : IOktaSessionManager
{
    private readonly ILogger<OktaSessionManager> _logger;
    private readonly IConfiguration _configuration;

    public OktaSessionManager(ILogger<OktaSessionManager> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SaveSession(OktaSession oktaSession, CancellationToken cancellationToken)
    {
        try
        {
            var sessionFile = FileHelper.GetSessionFile(_configuration);
            var payload = JsonSerializer.Serialize(oktaSession);
            await File.WriteAllTextAsync(sessionFile, payload, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error has occurred while saving session.");
        }
    }

    public async Task<OktaSession> GetSavedSession(CancellationToken cancellationToken)
    {
        try
        {
            var sessionFile = FileHelper.GetSessionFile(_configuration);
            if (!File.Exists(sessionFile)) return null;

            var rawFile = await File.ReadAllTextAsync(sessionFile, cancellationToken);
            return JsonSerializer.Deserialize<OktaSession>(rawFile);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error has occurred while getting saved session.");
            return null;
        }
    }

    public void DeleteSession()
    {
        try
        {
            var sessionFile = FileHelper.GetSessionFile(_configuration);
            if (!File.Exists(sessionFile)) return;
            
            File.Delete(sessionFile);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error has occurred while deleting session.");
        }
    }
}