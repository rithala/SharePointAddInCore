using Microsoft.AspNetCore.Authentication;

namespace SharePointAddInCore.LowTrust.Authentication
{
    public class SharePointAuthenticationOptions : AuthenticationSchemeOptions
    {
        public AuthenticationTarget Target { get; set; }

        public SharePointAuthenticationOptions()
        {
            Target = AuthenticationTarget.SPHost;
        }
    }

    public enum AuthenticationTarget
    {
        SPHost,
        SPWebApp
    }
}
