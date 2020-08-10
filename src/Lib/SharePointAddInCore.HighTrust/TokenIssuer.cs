using Microsoft.Extensions.Options;

using SharePointAddInCore.Core.SharePointContext;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;

namespace SharePointAddInCore.HighTrust
{
    internal interface ITokenIssuer
    {
        SharePointTokenResult GetS2SAccessTokenWithWindowsIdentity(string targetApplicationHostName, string targetRealm, WindowsIdentity identity);
    }

    internal class TokenIssuer : ITokenIssuer
    {
        private const string _nameIdentifierClaimType = "nameid";
        private const string _trustedForImpersonationClaimType = "trustedfordelegation";
        private const string _actorTokenClaimType = "actortoken";
        private const string _sharePointPrincipal = "00000003-0000-0ff1-ce00-000000000000";

        private static readonly TimeSpan _highTrustAccessTokenLifetime = TimeSpan.FromHours(12.0);

        private readonly HighTrustSharePointOptions _options;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;

        public TokenIssuer(IOptions<HighTrustSharePointOptions> options)
        {
            _options = options.Value;
            _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        }

        public SharePointTokenResult GetS2SAccessTokenWithWindowsIdentity(
            string targetApplicationHostName,
            string targetRealm,
            WindowsIdentity identity)
        {
            var claims = identity != null ? GetClaimsWithWindowsIdentity(identity) : null;
            return IssueToken(
                _options.ClientId,
                _options.IssuerId,
                targetRealm,
                _sharePointPrincipal,
                targetRealm,
                targetApplicationHostName,
                true,
                claims,
                claims == null);
        }

        private SharePointTokenResult IssueToken(
            string sourceApplication,
            string issuerApplication,
            string sourceRealm,
            string targetApplication,
            string targetRealm,
            string targetApplicationHostName,
            bool trustedForDelegation,
            IEnumerable<Claim> claims,
            bool appOnly = false)
        {
            if (null == _options.SigningCredentials)
            {
                throw new InvalidOperationException("SigningCredentials was not initialized");
            }

            var issuer = string.IsNullOrEmpty(sourceRealm) ? issuerApplication : string.Format("{0}@{1}", issuerApplication, sourceRealm);
            var nameid = string.IsNullOrEmpty(sourceRealm) ? sourceApplication : string.Format("{0}@{1}", sourceApplication, sourceRealm);
            var audience = string.Format("{0}/{1}@{2}", targetApplication, targetApplicationHostName, targetRealm);
            var expires = DateTime.UtcNow.Add(_highTrustAccessTokenLifetime);


            var actorTokenString = IssueActorToken(trustedForDelegation, appOnly, issuer, nameid, audience, expires);

            if (appOnly)
            {
                // App-only token is the same as actor token for delegated case
                return new SharePointTokenResult(actorTokenString, expires);
            }

            return new SharePointTokenResult(IssueOuterToken(claims, nameid, audience, actorTokenString, expires), expires);
        }

        private string IssueOuterToken(IEnumerable<Claim> claims, string nameid, string audience, string actorTokenString, DateTime expires)
        {
            var outerClaims = null == claims ? new List<Claim>() : new List<Claim>(claims);
            outerClaims.Add(new Claim(_actorTokenClaimType, actorTokenString));

            var jsonToken = new JwtSecurityToken(
                nameid, // outer token issuer should match actor token nameid
                audience,
                outerClaims,
                DateTime.UtcNow,
                expires);

            return _jwtSecurityTokenHandler.WriteToken(jsonToken);
        }

        private string IssueActorToken(bool trustedForDelegation, bool appOnly, string issuer, string nameid, string audience, DateTime expires)
        {
            var actorClaims = new List<Claim>
            {
                new Claim(_nameIdentifierClaimType, nameid)
            };

            if (trustedForDelegation && !appOnly)
            {
                actorClaims.Add(new Claim(_trustedForImpersonationClaimType, "true"));
            }

            // Create token
            var actorToken = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: actorClaims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: _options.SigningCredentials);

            return _jwtSecurityTokenHandler.WriteToken(actorToken);
        }

        private static Claim[] GetClaimsWithWindowsIdentity(WindowsIdentity identity) =>
            new Claim[]
            {
                new Claim(_nameIdentifierClaimType, identity.User.Value.ToLower()),
                new Claim("nii", "urn:office:idp:activedirectory")
            };
    }
}
