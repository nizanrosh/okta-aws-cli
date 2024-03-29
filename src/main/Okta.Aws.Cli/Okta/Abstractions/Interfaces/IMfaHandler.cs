﻿using Okta.Auth.Sdk;

namespace Okta.Aws.Cli.Okta.Abstractions.Interfaces
{
    public interface IMfaHandler
    {
        public string Type { get; }
        Task<IAuthenticationResponse> HandleMfa(IAuthenticationClient client, MfaParameters parameters, CancellationToken cancellationToken);
    }
}
