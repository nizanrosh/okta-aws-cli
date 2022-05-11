using Okta.Aws.Cli.GitHub.Abstractions;

namespace Okta.Aws.Cli.GitHub;

public interface IGitHubApiClient
{
    Task<GetTagsResponse> GetRepoTags(CancellationToken cancellationToken);
    Task<GetReleasesResponse> GetRepoReleases(CancellationToken cancellationToken);
    Task<bool> DownloadRelease(string url, string version, CancellationToken cancellationToken);
}