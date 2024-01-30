using Microsoft.Extensions.Configuration;
using Okta.Aws.Cli.Cli.Interfaces;
using Okta.Aws.Cli.Helpers;

namespace Okta.Aws.Cli.Cli;

public class LogoutArgumentHandler : ILogoutArgumentHandler
{
    private readonly IConfiguration _configuration;

    public LogoutArgumentHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public Task Handle(CancellationToken cancellationToken)
    {
        var userSettingsFolder = FileHelper.GetUserSettingsFolder(_configuration);
        var sessionFilePath = $"{userSettingsFolder}/session.json"; 
        if (!File.Exists(sessionFilePath)) return Task.CompletedTask;
        
        File.Delete(sessionFilePath);
        
        return Task.CompletedTask;
    }
}