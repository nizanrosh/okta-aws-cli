using Cocona;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Okta.Aws.Cli.Abstractions;
using Okta.Aws.Cli.Cli.Configurations;
using Okta.Aws.Cli.Cli.Interfaces;
using Okta.Aws.Cli.Constants;

namespace Okta.Aws.Cli.Extensions;

public static class CoconaExtensions
{
    public static CoconaApp AddWhoAmICommand(this CoconaApp app, CancellationToken cancellationToken)
    {
        app.AddCommand(Commands.WhoAmI,
            [IgnoreUnknownOptions] async (IWhoAmIArgumentHandler handler) => await handler.Handle(cancellationToken))
            .WithDescription("Lists the current user.");

        return app;
    }

    public static CoconaApp AddSelectCommand(this CoconaApp app, IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var selectHandler = serviceProvider.GetRequiredService<ISelectArgumentHandler>();

        app.AddCommand(Commands.Select, [IgnoreUnknownOptions] async () =>
        {
            var profileName = configuration[User.Settings.ProfileName] ?? "default";
            await selectHandler.Handle(profileName, cancellationToken);
        }).WithDescription("Select from available credentials.").OptionLikeCommand(builder => builder
            .Add(Commands.Sub.Profile, new[] { Commands.Sub.ShortProfile },
                [IgnoreUnknownOptions] async ([Argument("The AWS profile.")] string profile) =>
                await selectHandler.Handle(profile, cancellationToken)));

        return app;
    }

    public static CoconaApp AddResetCommand(this CoconaApp app, CancellationToken cancellationToken)
    {
        app.AddCommand(Commands.Reset,
            async (IResetArgumentHandler handler) => await handler.Handle(cancellationToken))
            .WithDescription("Removes *all* user settings.");

        return app;
    }

    public static CoconaApp AddLogoutCommand(this CoconaApp app, CancellationToken cancellationToken)
    {
        app.AddCommand(Commands.Logout, [IgnoreUnknownOptions] async (ILogoutArgumentHandler handler) =>
            await handler.Handle(cancellationToken)).WithDescription("Logs out the current user.");

        return app;
    }

    public static CoconaApp AddConfigureCommand(this CoconaApp app, CancellationToken cancellationToken)
    {
        app.AddCommand(Commands.Configure,
            [IgnoreUnknownOptions] async (IConfigureArgumentHandler handler) =>
                await handler.Handle(cancellationToken))
            .WithDescription("Configure user settings.");

        return app;
    }

    public static CoconaApp AddRunCommand(this CoconaApp app, IServiceProvider provider,
        CancellationToken cancellationToken)
    {
        var runArgumentHandler = provider.GetRequiredService<IRunArgumentHandler>();
        var configuration = provider.GetRequiredService<IConfiguration>();
        var userSettingsHandler = provider.GetRequiredService<IUserSettingsHandler>();

        app.AddCommand(Commands.Run,
            [IgnoreUnknownOptions] async () =>
            {
                var runConfig = BuildRunConfiguration(configuration, Commands.Run);
                await runArgumentHandler.Handle(runConfig, cancellationToken);
            }).OptionLikeCommand(x =>
        {
            x.Add(Commands.Sub.Save, new[] { Commands.Sub.ShortSave },[IgnoreUnknownOptions] async () =>
            {
                var runConfig = BuildRunConfiguration(configuration, Commands.Sub.Save);
                var credentials = await runArgumentHandler.Handle(runConfig, cancellationToken);

                configuration[User.Settings.DefaultAwsAccount] = credentials.SelectedAwsAccount;
                configuration[User.Settings.DefaultAwsAccountAlias] = credentials.SelectedAwsAccountAlias;
                configuration[User.Settings.DefaultAwsRole] = credentials.FullArn;
                configuration[User.Settings.DefaultAwsRoleAlias] = credentials.FullArnAlias;
                await userSettingsHandler.SaveCurrentUserSettingsToFile(cancellationToken);
            }).WithDescription("Saves your selections for future runs.");

            x.Add(Commands.Sub.SessionTime, new[] { Commands.Sub.ShortSessionTime }, [IgnoreUnknownOptions]
                async ([Argument] int sessionTime) =>
                {
                    var runConfig = BuildRunConfiguration(configuration, Commands.Sub.SessionTime, sessionTime);
                    await runArgumentHandler.Handle(runConfig, cancellationToken);
                }).WithDescription("Role session time duration in seconds.");

            x.Add(Commands.Sub.Fresh, new[] { Commands.Sub.ShortFresh }, [IgnoreUnknownOptions] async () =>
            {
                var runConfig = BuildRunConfiguration(configuration, Commands.Sub.Fresh);
                await runArgumentHandler.Handle(runConfig, cancellationToken);
            }).WithDescription("Run without saved selections.");
        });

        return app;
    }

    private static RunConfiguration BuildRunConfiguration(IConfiguration configuration, string subCommand,
        int sessionTime = 0)
    {
        var userSettings = configuration.GetSection(nameof(UserSettings)).Get<UserSettings>();
        var defaultAwsAccount = userSettings.DefaultAwsAccount;
        var defaultAwsAccountAlias = userSettings.DefaultAwsAccountAlias;
        var defaultAwsRole = userSettings.DefaultAwsRole;
        var defaultAwsRoleAlias = userSettings.DefaultAwsRoleAlias;

        if (!string.IsNullOrEmpty(defaultAwsAccount) && !string.IsNullOrEmpty(defaultAwsRole))
        {
            if (string.IsNullOrEmpty(defaultAwsAccountAlias)) defaultAwsAccountAlias = defaultAwsAccount;
            if (string.IsNullOrEmpty(defaultAwsRoleAlias)) defaultAwsRoleAlias = defaultAwsRole;

            return new RunConfiguration(sessionTime, defaultAwsAccount, defaultAwsAccountAlias, defaultAwsRole,
                defaultAwsRoleAlias, subCommand);
        }

        return new RunConfiguration(sessionTime, subCommand);
    }
}