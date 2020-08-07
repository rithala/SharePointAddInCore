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
        private SharePointContextToken()
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

        public string StsAuthority => (new Uri(SecurityTokenServiceUri)).Authority;

        public Uri AcsGlobalEndpointUrl => new Uri(string.Format(CultureInfo.InvariantCulture, "https://{0}.{1}/", GlobalEndPointPrefix, AcsHostUrl));

        private string GlobalEndPointPrefix => StsAuthority.Substring(0, AcsFirstDot);

        private string AcsHostUrl => StsAuthority.Substring(AcsFirstDot + 1);

        private int AcsFirstDot => StsAuthority.IndexOf('.');

        private Dictionary<string, object> AppCtx => GetClaimValue("appctx") != null
            ? JsonConvert.DeserializeObject<Dictionary<string, object>>(GetClaimValue("appctx"))
            : null;

        private string GetClaimValue(string claimType) => Claims.FirstOrDefault(x => x.Type == claimType)?.Value;
    }
}
