using Newtonsoft.Json;

using SharePointAddInCore.Core.Authentication;

using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace SharePointAddInCore.Core.SharePointContext
{
    public class SharePointUserTokenResult : SharePointTokenResult
    {
        public SharePointContextUser User { get; }

        [JsonConstructor]
        internal SharePointUserTokenResult(
            string accessToken,
            DateTime expires,
            SharePointContextUser user) : base(accessToken, expires)
        {
            User = user;
        }

        internal ClaimsPrincipal ToClaimsPrincipal(string authenticationType, IEnumerable<string> roles = null)
        {
            var claimsIdentity = new ClaimsIdentity(authenticationType);

            claimsIdentity.AddClaims(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, User.Id.ToString()),
                new Claim(ClaimTypes.Upn, User.UserPrincipalName),
                new Claim(ClaimTypes.Name, User.Title),
                new Claim(ClaimTypes.Email, User.Email),
                new Claim(SharePointAuthentication.AccessTokenClaim, AccessToken),
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
