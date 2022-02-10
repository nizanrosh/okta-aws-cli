using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Okta.Aws.Cli.Abstractions;
using Okta.Aws.Cli.Constants;
using Okta.Aws.Cli.Helpers;
using Okta.Aws.Cli.Okta.Abstractions;
using Sharprompt;

namespace Okta.Aws.Cli;

public class UserSettingsHandler : IUserSettingsHandler
{
    private readonly IConfiguration _configuration;

    public UserSettingsHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task ConfigureUserSettingsFile(CancellationToken cancellationToken)
    {
        var userSettingsFolder = FileHelper.GetUserSettingsFolder(_configuration);
        var userSettingsFile = FileHelper.GetUserSettingsFile(_configuration);
        if (!Directory.Exists(userSettingsFolder)) Directory.CreateDirectory(userSettingsFolder);

        var userSettings = CreateUserSettings();
        var payload = JsonConvert.SerializeObject(new UserSettingsWrapper(userSettings));
        await File.WriteAllTextAsync(userSettingsFile, payload, cancellationToken);
    }

    private UserSettings CreateUserSettings()
    {
        var userSettings = _configuration.GetSection(nameof(UserSettings)).Get<UserSettings>() ?? new UserSettings();

        userSettings.OktaDomain = Prompt.Input<string>("Enter your Okta domain", userSettings.OktaDomain);
        userSettings.Username = Prompt.Input<string>("Enter your Okta username", userSettings.Username);
        userSettings.Password = Prompt.Password("Enter your Okta password");

        if (Enum.TryParse(typeof(MfaTypes), userSettings.MfaType, false, out var mfaType))
        {
            userSettings.MfaType = Prompt.Select<MfaTypes>("Enter your MFA type", defaultValue: mfaType).ToString();
        }
        else
        {
            userSettings.MfaType = Prompt.Select<MfaTypes>("Enter your MFA type").ToString();
        }

        userSettings.ProfileName = Prompt.Input<string>("Enter the desired AWS profile name", userSettings.ProfileName);
        userSettings.Region = Prompt.Input<string>("Enter your AWS region", userSettings.Region);

        return userSettings;
    }

    public void SanityCheck()
    {
        var userSettings = _configuration.GetSection(nameof(UserSettings)).Get<UserSettings>();

        if (userSettings?.OktaDomain == null)
        {
            PromptForParameter("Enter your Okta domain", User.Settings.OktaDomain);
        }

        if (userSettings?.Username == null)
        {
            PromptForParameter("Enter your Okta username", User.Settings.Username);
        }

        if (userSettings?.Password == null)
        {
            PromptForParameter("Enter your Okta password", User.Settings.Password, true);
        }

        if (userSettings?.MfaType == null)
        {
            PromptForParameter<MfaTypes>("Enter your MFA type", User.Settings.MfaType);
        }

        if (userSettings?.ProfileName == null)
        {
            PromptForParameter("Enter the desired AWS profile name", User.Settings.ProfileName, false, "default");
        }

        if (userSettings?.Region == null)
        {
            PromptForParameter("Enter your AWS region", User.Settings.Region, false, "eu-west-1");
        }
    }

    private void PromptForParameter(string message, string configPath, bool isPassword = false, string? defaultValue = null, string? placeholder = null)
    {
        string param;

        if (isPassword)
        {
            param = Prompt.Password(message);
        }
        else
        {
            param = Prompt.Input<string>(message, defaultValue, placeholder);
        }
        _configuration[configPath] = param;
    }

    private void PromptForParameter<T>(string message, string configPath)
    {
        var param = Prompt.Select<T>(message)!.ToString();
        _configuration[configPath] = param;
    }
}