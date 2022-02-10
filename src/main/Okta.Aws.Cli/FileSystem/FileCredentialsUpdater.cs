using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Okta.Aws.Cli.Aws.Abstractions;
using Okta.Aws.Cli.Constants;
using Okta.Aws.Cli.Helpers;

namespace Okta.Aws.Cli.FileSystem
{

    public class FileCredentialsUpdater : ICredentialsUpdater
    {
        private readonly ILogger<FileCredentialsUpdater> _logger;
        private readonly IConfiguration _config;

        public FileCredentialsUpdater(ILogger<FileCredentialsUpdater> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public Task UpdateCredentials(AwsCredentials credentials, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating local credentials file.");

            var username = _config[LocalSystem.Username];

            var folderPath = FileHelper.GetUserAwsFolder(_config);
            var filePath = FileHelper.GetUserAwsCredentialsFile(_config);

            if (!Directory.Exists(folderPath))
            {
                CreateDirectory(folderPath);
                return CreateCredentialsFile(filePath, credentials, cancellationToken);
            }

            if (!File.Exists(filePath))
            {
                return CreateCredentialsFile(filePath, credentials, cancellationToken);
            }

            return UpdateFile(filePath, credentials, cancellationToken);
        }

        private async Task UpdateFile(string filePath, AwsCredentials credentials, CancellationToken cancellationToken)
        {
            var cacheFolder = FileHelper.GetUserSettingsFolder(_config);
            var cacheFile = FileHelper.GetUserAwsBackupFile(_config);

            CreateDirectory(cacheFolder);
            File.Copy(filePath, cacheFile, true);

            var credentialsFile = await File.ReadAllLinesAsync(filePath, cancellationToken);
            var lines = GetFileLines(credentials);

            var profileName = _config[User.Settings.ProfileName];

            if (!credentialsFile.Contains($"[{profileName}]"))
            {
                lines.Insert(0, "\n");
                await File.AppendAllLinesAsync(filePath, lines, cancellationToken);
                return;
            }

            var profileToOverrideStartIndex = Array.FindIndex(credentialsFile, f => f == $"[{profileName}]");
            var profileToOverrideEndIndex = Array.FindIndex(credentialsFile, profileToOverrideStartIndex + 1, f => f.StartsWith("["));

            var linesToPreserve = credentialsFile.TakeWhile(l => l != $"[{profileName}]");
            var fileWithNewCredentials = linesToPreserve.Concat(lines);

            if (profileToOverrideEndIndex == -1)
            {
                await File.WriteAllLinesAsync(filePath, fileWithNewCredentials, cancellationToken);
                return;
            }

            var fileLastLinesToPreserve = credentialsFile.Skip(profileToOverrideEndIndex);

            var endResultFile = fileWithNewCredentials.Concat(fileLastLinesToPreserve);
            await File.WriteAllLinesAsync(filePath, endResultFile, cancellationToken);
        }

        private Task CreateCredentialsFile(string path, AwsCredentials credentials, CancellationToken cancellationToken)
        {
            var lines = GetFileLines(credentials);
            return File.WriteAllLinesAsync(path, lines, cancellationToken);
        }

        private void CreateDirectory(string folderPath)
        {
            if (Directory.Exists(folderPath)) return;

            Directory.CreateDirectory(folderPath);
        }

        private List<string> GetFileLines(AwsCredentials credentials)
        {
            var lines = new List<string>
            {
                $"[{_config[User.Settings.ProfileName]}]",
                $"aws_access_key_id={credentials.AccessKeyId}",
                $"aws_secret_access_key={credentials.SecretAccessKey}",
                $"region={credentials.Region}",
                $"aws_session_token={credentials.SessionToken}"
            };

            return lines;
        }
    }
}
