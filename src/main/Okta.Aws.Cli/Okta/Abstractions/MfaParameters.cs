namespace Okta.Aws.Cli.Okta.Abstractions
{
    public class MfaParameters
    {
        public string FactorId { get; set; }
        public string StateToken { get; set; }

        public MfaParameters(string factorId, string stateToken) => (FactorId, StateToken) = (factorId, stateToken);
    }
}
