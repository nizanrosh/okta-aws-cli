﻿using System.ComponentModel.DataAnnotations;
using System.Text;
using Amazon;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Okta.Aws.Cli.Abstractions;
using Okta.Aws.Cli.Constants;
using Okta.Aws.Cli.Encryption;
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
        if (!Directory.Exists(userSettingsFolder)) Directory.CreateDirectory(userSettingsFolder!);

        var userSettings = CreateUserSettings();
        var payload = JsonConvert.SerializeObject(new UserSettingsWrapper(userSettings));
        await File.WriteAllTextAsync(userSettingsFile, payload, cancellationToken);
    }

    public Task SaveCurrentUserSettingsToFile(CancellationToken cancellationToken)
    {
        var userSettingsFolder = FileHelper.GetUserSettingsFolder(_configuration);
        var userSettingsFile = FileHelper.GetUserSettingsFile(_configuration);
        if (!Directory.Exists(userSettingsFolder)) Directory.CreateDirectory(userSettingsFolder!);

        var userSettings = _configuration.GetSection(nameof(UserSettings)).Get<UserSettings>();
        var payload = JsonConvert.SerializeObject(new UserSettingsWrapper(userSettings));
        return File.WriteAllTextAsync(userSettingsFile, payload, cancellationToken);
    }

    private UserSettings CreateUserSettings()
    {
        var userSettings = _configuration.GetSection(nameof(UserSettings)).Get<UserSettings>() ?? new UserSettings();

        var url = Prompt.Input<string>("Enter your Okta domain", userSettings.OktaDomain);
        while (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            url = Prompt.Input<string>("The input is not a valid url, please enter a valid url",
                userSettings.OktaDomain);
        }

        userSettings.OktaDomain = url;

        userSettings.Username = Prompt.Input<string>("Enter your Okta username", userSettings.Username);
        userSettings.Password = AesOperation.EncryptString(Prompt.Password("Enter your Okta password",
            validators: new[] { Validators.Required() }));

        if (Enum.TryParse(typeof(MfaTypes), userSettings.MfaType, false, out var mfaType))
        {
            userSettings.MfaType = Prompt.Select<MfaTypes>("Enter your MFA type", defaultValue: mfaType).ToString();
        }
        else
        {
            userSettings.MfaType = Prompt.Select<MfaTypes>("Enter your MFA type").ToString();
        }

        userSettings.ProfileName = Prompt.Input<string>("Enter the desired AWS profile name", userSettings.ProfileName);
        //userSettings.ProfileNames = Prompt.List<string>(configure =>
        //{
        //    configure.Message = "Enter AWS profile name(s), press enter when done";
        //    configure.DefaultValues = userSettings.ProfileNames;
        //    configure.Maximum = 10;
        //});
        var region = Prompt.Input<string>("Enter your AWS region", userSettings.Region);
        var regionResult = RegionEndpoint.GetBySystemName(region);
        while (regionResult.DisplayName == "Unknown")
        {
            region = Prompt.Input<string>("The input is not a valid aws region, please enter a valid region",
                userSettings.Region);
            regionResult = RegionEndpoint.GetBySystemName(region);
        }

        userSettings.Region = region;
        return userSettings;
    }

    public void PrettyPrint(UserSettings userSettings)
    {
        Console.WriteLine($"Okta Domain: {userSettings.OktaDomain}");
        Console.WriteLine($"Username: {userSettings.Username}");
        Console.WriteLine($"MFA type: {userSettings.MfaType}");
        Console.WriteLine($"AWS profile: {userSettings.ProfileName}");
        Console.WriteLine($"AWS region: {userSettings.Region}");
    }

    public void SanityCheck()
    {
        var userSettings = _configuration.GetSection(nameof(UserSettings)).Get<UserSettings>();

        if (string.IsNullOrEmpty(userSettings?.OktaDomain))
        {
            PromptForParameter("Enter your Okta domain", User.Settings.OktaDomain);
        }

        if (string.IsNullOrEmpty(userSettings?.Username))
        {
            PromptForParameter("Enter your Okta username", User.Settings.Username);
        }

        if (string.IsNullOrEmpty(userSettings?.Password))
        {
            PromptForParameter("Enter your Okta password", User.Settings.Password, true);
        }
        else if (!IsBase64String(userSettings.Password))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Your password is not encrypted, run oacli configure to encrypt.");
            Console.ForegroundColor = ConsoleColor.White;
            _configuration[User.Settings.Password] = AesOperation.EncryptString(userSettings.Password);
        }

        if (string.IsNullOrEmpty(userSettings?.MfaType))
        {
            PromptForParameter<MfaTypes>("Enter your MFA type", User.Settings.MfaType);
        }

        if (string.IsNullOrEmpty(userSettings?.ProfileName))
        {
            PromptForParameter("Enter the desired AWS profile name", User.Settings.ProfileName, false, "default");
        }

        if (string.IsNullOrEmpty(userSettings?.Region))
        {
            PromptForParameter("Enter your AWS region", User.Settings.Region, false, "eu-west-1");
        }
    }

    private void PromptForParameter(string message, string configPath, bool isPassword = false,
        string defaultValue = null, string placeholder = null)
    {
        string param;

        if (isPassword)
        {
            param = AesOperation.EncryptString(Prompt.Password(message));
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

    public bool IsBase64String(string base64)
    {
        var buffer = new Span<byte>(new byte[base64.Length]);
        return Convert.TryFromBase64String(base64, buffer, out _);
    }
}