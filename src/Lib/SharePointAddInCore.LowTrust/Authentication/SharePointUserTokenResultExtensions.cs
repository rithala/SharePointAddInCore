using SharePointAddInCore.Core.SharePointContext;

using System.Collections.Generic;
using System.Security.Claims;

namespace SharePointAddInCore.LowTrust.Authentication
{
    internal static class SharePointUserTokenResultExtensions
    {
        internal static ClaimsPrincipal ToClaimsPrincipal(this SharePointUserTokenResult tokenResult, string authenticationType, IEnumerable<string> roles = null)
        {
            var claimsIdentity = new ClaimsIdentity(authenticationType);

            claimsIdentity.AddClaims(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, tokenResult.User.Id.ToString()),
                new Claim(ClaimTypes.Upn, tokenResult.User.UserPrincipalName),
                new Claim(ClaimTypes.Name, tokenResult.User.Title),
                new Claim(ClaimTypes.Email, tokenResult.User.Email),
                new Claim(SharePointAuthentication.AccessTokenClaim, tokenResult.AccessToken),
            });

            if (roles != null)
            {
                foreach (var role in roles)
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
            }

            return new ClaimsPrincipal(claimsIdentity);
        }
    }
}
