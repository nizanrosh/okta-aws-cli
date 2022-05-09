using System.Net.Http.Json;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Okta.Aws.Cli.GitHub.Abstractions;

namespace Okta.Aws.Cli.GitHub;

public class GitHubApiClient : IGitHubApiClient
{
    private readonly ILogger<GitHubApiClient> _logger;
    private readonly HttpClient _httpClient;

    public GitHubApiClient(ILogger<GitHubApiClient> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<GetTagsResponse> GetRepoTags(CancellationToken cancellationToken)
    {
        try
        {
            var request =
                new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/nizanrosh/okta-aws-cli/tags")
                {
                    Headers = { { "Accept", "application/vnd.github.v3+json" }, {"User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36"} }
                };

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Request failed, status code: {response.StatusCode}.");
                return new GetTagsResponse();
            }

            return new GetTagsResponse
            {
                Tags = await response.Content.ReadFromJsonAsync<TagMetadata[]>(cancellationToken: cancellationToken)
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error has occurred while trying to get tags from GitHub Api.");
            return new GetTagsResponse();
        }
    }

    public async Task<GetReleasesResponse> GetRepoReleases(CancellationToken cancellationToken)
    {
        try
        {
            var request =
                new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/nizanrosh/okta-aws-cli/releases")
                {
                    Headers = { { "Accept", "application/vnd.github.v3+json" }, { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36" } }
                };

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Request failed, status code: {response.StatusCode}.");
                return new GetReleasesResponse();
            }

            return new GetReleasesResponse
            {
                Releases = await response.Content.ReadFromJsonAsync<ReleaseMetadata[]>(cancellationToken: cancellationToken)
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error has occurred while trying to get releases from GitHub Api.");
            return new GetReleasesResponse();
        }
    }

    public async Task<bool> DownloadRelease(string url, string version, CancellationToken cancellationToken)
    {
        try
        {
            var request =
                new HttpRequestMessage(HttpMethod.Get, url)
                {
                    Headers = { { "Accept", "application/vnd.github.v3+json" }, { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36" } }
                };

            var response = await _httpClient.SendAsync(request, cancellationToken);
            await using var fs = new FileStream($"release/{version}.zip", FileMode.CreateNew);
            await response.Content.CopyToAsync(fs, cancellationToken);

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error has occurred while trying to download a release from GitHub Api.");
            return false;
        }
    }
}