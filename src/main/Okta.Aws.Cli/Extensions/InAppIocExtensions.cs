using Microsoft.Extensions.DependencyInjection;
using Okta.Aws.Cli.Aws;
using Okta.Aws.Cli.Cli;
using Okta.Aws.Cli.Cli.Interfaces;
using Okta.Aws.Cli.FileSystem;
using Okta.Aws.Cli.Okta;
using Okta.Aws.Cli.Okta.Abstractions;
using Okta.Aws.Cli.Okta.MFA;
using Okta.Aws.Cli.Okta.Saml;

namespace Okta.Aws.Cli.Extensions
{
    public static class InAppIocExtensions
    {
        public static IServiceCollection AddOktaSamlProvider(this IServiceCollection services)
        {
            services.AddSingleton<IMfaFactory, MfaFactory>();
            services.AddSingleton<IMfaHandler, MfaPushHandler>();
            services.AddSingleton<IMfaHandler, MfaSmsHandler>();

            services.AddSingleton<IOktaAuthenticator, OktaAuthenticator>();
            services.AddSingleton<IOktaSamlProvider, OktaSamlProvider>();
            services.AddSingleton<IOktaApiClient, OktaApiClient>();
            services.AddSingleton<ISamlExtractor, SamlExtractor>();

            return services;
        }

        public static IServiceCollection AddAwsCredentialsProvider(this IServiceCollection services)
        {
            services.AddSingleton<IAwsCredentialsProvider, AwsCredentialsProvider>();

            return services;
        }

        public static IServiceCollection AddFileCredentialsUpdater(this IServiceCollection services)
        {
            services.AddSingleton<ICredentialsUpdater, FileCredentialsUpdater>();

            return services;
        }

        public static IServiceCollection AddFileVersionUpdater(this IServiceCollection services)
        {
            services.AddSingleton<IFileVersionUpdater, FileVersionUpdater>();

            return services;
        }

        public static IServiceCollection AddFileSystemUpdaters(this IServiceCollection services)
        {
            services.AddFileCredentialsUpdater();
            services.AddFileVersionUpdater();

            return services;
        }

        public static IServiceCollection AddCliArgumentHandling(this IServiceCollection services)
        {
            services.AddSingleton<ICliArgumentFactory, CliArgumentFactory>();

            services.AddSingleton<ICliArgumentHandler, RunArgumentHandler>();
            services.AddSingleton<ICliArgumentHandler, ConfigureArgumentHandler>();
            services.AddSingleton<ICliArgumentHandler, VersionArgumentHandler>();
            //services.AddSingleton<ICliArgumentHandler, UpdateArgumentHandler>();

            services.AddSingleton<IInvalidArgumentHandler, InvalidArgumentHandler>();

            return services;
        }
    }
}
