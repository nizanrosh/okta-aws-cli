﻿using Okta.Auth.Sdk;
using Okta.Aws.Cli.Okta.Abstractions;
using Okta.Aws.Cli.Okta.Abstractions.Interfaces;

namespace Okta.Aws.Cli.Okta.MFA
{
    public abstract class MfaHandlerBase : IMfaHandler
    {
        public abstract string Type { get; }
        public abstract Task<IAuthenticationResponse> HandleMfa(IAuthenticationClient client, MfaParameters parameters, CancellationToken cancellationToken);
    }
}
