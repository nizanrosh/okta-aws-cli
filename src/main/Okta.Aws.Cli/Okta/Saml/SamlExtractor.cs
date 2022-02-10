using System.Net;
using System.Net.Http.Json;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Okta.Aws.Cli.Abstractions;
using Okta.Aws.Cli.Constants;
using Okta.Aws.Cli.Okta.Abstractions;
using Okta.Aws.Cli.Okta.Constants;

namespace Okta.Aws.Cli.Okta.Saml
{
    public interface ISamlExtractor
    {
        Task<SamlResponse> ExtractSamlFromHtml(string sessionToken);
    }

    public class SamlExtractor : ISamlExtractor
    {
        private readonly ILogger<SamlExtractor> _logger;
        private readonly IConfiguration _configuration;

        public SamlExtractor(ILogger<SamlExtractor> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<SamlResponse> ExtractSamlFromHtml(string sessionToken)
        {
            _logger.LogInformation("Extracting SAML assertion.");

            var settings = _configuration.GetSection(nameof(UserSettings)).Get<UserSettings>();

            var cookieContainer = new CookieContainer();
            var httpClientHandler = new HttpClientHandler
            {
                CookieContainer = cookieContainer
            };

            var httpClient = new HttpClient(httpClientHandler);

            var sessionRequest = new SessionRequest(sessionToken);
            var payload = JsonConvert.SerializeObject(sessionRequest);

            var sessionResponse = await httpClient.PostAsJsonAsync($"{settings.OktaDomain}/api/v1/sessions", sessionRequest);

            var sessionContent = await sessionResponse.Content.ReadAsStringAsync();
            var sessionModel = JsonConvert.DeserializeObject<SessionResponse>(sessionContent);

            if (string.IsNullOrEmpty(settings.AppUrl))
            {
                settings.AppUrl = await GetAppUrl(httpClient, sessionModel.Id);
            }

            cookieContainer.Add(new Uri(settings.OktaDomain!), new Cookie("sid", sessionModel.Id));

            var awsResponse = await httpClient.GetAsync(settings.AppUrl);

            var html = await awsResponse.Content.ReadAsStringAsync();

            var samlToken = ExtractFromHtml(html);

            return new SamlResponse(WebUtility.HtmlDecode(samlToken));
        }

        private async Task<string> GetAppUrl(HttpClient httpClient, string sessionId)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{_configuration[User.Settings.OktaDomain]}/api/v1/users/me/appLinks");
            httpRequest.Headers.Add("Cookie", $"sid={sessionId}");

            var response = await httpClient.SendAsync(httpRequest);
            var content = await response.Content.ReadAsStringAsync();

            var appLinks = JsonConvert.DeserializeObject<AppLinks[]>(content);

            return appLinks?.FirstOrDefault(al => al.AppName == AppNames.Amazon)?.LinkUrl;
        }

        private string ExtractFromHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var samlAttribute = doc.DocumentNode.SelectNodes("//form//input").FirstOrDefault();
            var samlToken = samlAttribute.GetAttributeValue("value", null);

            return samlToken;
        }
    }
}
