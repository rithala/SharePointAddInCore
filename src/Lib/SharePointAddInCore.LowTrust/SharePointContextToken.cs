using Microsoft.IdentityModel.Tokens;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace SharePointAddInCore.LowTrust
{
    internal class SharePointContextToken : JwtSecurityToken
    {
        [JsonConstructor]
        private SharePointContextToken(IEnumerable<TokenClaim> claims)
            : base(claims: claims.Select(x => new System.Security.Claims.Claim(x.Type, x.Value)))
        {
        }

        public SharePointContextToken(SecurityToken securityToken)
            : this((JwtSecurityToken)securityToken)
        {
        }

        public SharePointContextToken(JwtSecurityToken securityToken)
            : base(
                securityToken.Issuer,
                securityToken.Audiences.FirstOrDefault(),
                securityToken.Claims,
                securityToken.ValidFrom,
                securityToken.ValidTo,
                securityToken.SigningCredentials)
        {
        }

        public string NameId => GetClaimValue("nameid");
        public string RefreshToken => GetClaimValue("refreshtoken");
        public string TargetPrincipalName => GetClaimValue("appctxsender")?.Split('@')[0];
        public string CacheKey => AppCtx != null
            ? AppCtx["CacheKey"].ToString()
            : null;
        public string SecurityTokenServiceUri => AppCtx != null
            ? AppCtx["SecurityTokenServiceUri"].ToString()
            : null;

        public string Realm => Audiences.FirstOrDefault()?.Substring(Audiences.FirstOrDefault().IndexOf('@') + 1);

        private Dictionary<string, object> AppCtx => GetClaimValue("appctx") != null
            ? JsonConvert.DeserializeObject<Dictionary<string, object>>(GetClaimValue("appctx"))
            : null;

        private string GetClaimValue(string claimType) => Claims.FirstOrDefault(x => x.Type == claimType)?.Value;

        private class TokenClaim
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }
    }
}
