using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

using System;

namespace SharePointAddInCore.LowTrust.Authentication
{
    public static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddSharePointAddIn(this AuthenticationBuilder authenticationBuilder, Action<SharePointAuthenticationOptions> configureOptions = null)
        {
            return authenticationBuilder.AddScheme<SharePointAuthenticationOptions, SharePointAuthenticationHandler>(
                SharePointAuthentication.SchemeName,
                configureOptions);
        }
    }
}
