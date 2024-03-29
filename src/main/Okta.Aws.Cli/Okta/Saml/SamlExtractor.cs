﻿using System.Linq.Expressions;
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
    public class SamlExtractor : ISamlExtractor
    {
        private readonly ILogger<SamlExtractor> _logger;
        private readonly IConfiguration _configuration;

        public SamlExtractor(ILogger<SamlExtractor> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public SamlResult ExtractSamlFromHtml(SamlHtmlResponse samlHtmlResponse)
        {
            return GetSamlExtractorResult(samlHtmlResponse);
        }

        private SamlResult GetSamlExtractorResult(SamlHtmlResponse samlHtmlResponse)
        {
            var selectedSaml = ExtractFromHtml(samlHtmlResponse.SelectedSaml);
            ArgumentNullException.ThrowIfNull(selectedSaml, nameof(selectedSaml));

            if (samlHtmlResponse.AdditionalSamls == null || !samlHtmlResponse.AdditionalSamls.Any()) return new SamlResult(new Abstractions.Saml(WebUtility.HtmlDecode(selectedSaml)));

            var additionalSamls = new List<Abstractions.Saml>();
            
            foreach (var additionalSaml in samlHtmlResponse.AdditionalSamls)
            {
                var extractedAdditionalSaml = ExtractFromHtml(additionalSaml);
                if(string.IsNullOrEmpty(extractedAdditionalSaml)) continue;
                additionalSamls.Add(new Abstractions.Saml(WebUtility.HtmlDecode(extractedAdditionalSaml)));
            }

            return new SamlResult(new Abstractions.Saml(WebUtility.HtmlDecode(selectedSaml)), additionalSamls);
        }

        private string ExtractFromHtml(string html)
        {
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var samlAttribute = doc.DocumentNode.SelectNodes("//form//input").FirstOrDefault();
                var samlToken = samlAttribute?.GetAttributeValue("value", null);
                ArgumentNullException.ThrowIfNull(samlToken, nameof(samlToken));

                return samlToken;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed extracting SAML from HTML.");
                return null;
            }
        }
    }

    public class SamlResult
    {
        public Abstractions.Saml SelectedSaml { get; }
        public IReadOnlyCollection<Abstractions.Saml> AdditionalSamls { get; } = Array.Empty<Abstractions.Saml>();
        
        public SelectedAppUrl SelectedAppUrl { get; set; }

        public SamlResult(Abstractions.Saml selectedSaml, IReadOnlyCollection<Abstractions.Saml> additionalSamls = null)
        {
            SelectedSaml = selectedSaml;
            
            if(additionalSamls != null)
                AdditionalSamls = additionalSamls;
        }
    }
}
