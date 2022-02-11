using System.ComponentModel.DataAnnotations;

namespace Okta.Aws.Cli.Okta.Abstractions
{
    public enum MfaTypes
    {
        [Display(Name = "push")]
        push,
        [Display(Name = "sms")]
        sms
    }
}
