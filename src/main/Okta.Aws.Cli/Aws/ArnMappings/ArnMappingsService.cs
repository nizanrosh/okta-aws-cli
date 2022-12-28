using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Okta.Aws.Cli.Abstractions;
using Okta.Aws.Cli.Helpers;

namespace Okta.Aws.Cli.Aws.ArnMappings;

public class ArnMappingsService : IArnMappingsService
{
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, string> _arnMappings;

    public ArnMappingsService(IConfiguration configuration)
    {
        _configuration = configuration;

        var rawArnMappings = _configuration.GetSection("ArnMappings").Get<List<ArnMapping>>();
        if (rawArnMappings == null)
        {
            _arnMappings = new Dictionary<string, string>();
        }
        else
        {
            _arnMappings = rawArnMappings.ToDictionary(x => x.Key, x => x.Value);
        }
    }

    public Task UpdateArnMappingsFile(CancellationToken cancellationToken)
    {
        var mappings = _arnMappings.Select(m => new ArnMapping { Key = m.Key, Value = m.Value }).ToList();

        var userSettingsFolder = FileHelper.GetUserSettingsFolder(_configuration);
        var userSettingsFile = FileHelper.GetArnMappingsFile(_configuration);

        if (!Directory.Exists(userSettingsFolder)) Directory.CreateDirectory(userSettingsFolder);

        var payload = JsonConvert.SerializeObject(new ArnMappingsWrapper(mappings));
        return File.WriteAllTextAsync(userSettingsFile, payload, cancellationToken);
    }

    public Dictionary<string, string> GetArnMappings()
    {
        return _arnMappings;
    }
}