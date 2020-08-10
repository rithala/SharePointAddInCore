using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using SharePointAddInCore.Core.SharePointContext;
using SharePointAddInCore.LowTrust.AzureAccessControl;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SharePointAddInCore.LowTrust
{
    internal class LowTrustSharePointContext : SharePointContextBase, ISharePointContext
    {
        private const string _contextTokenKey = "SPContextToken";

        private const string _tokenAppWebKey = "SPAppTokenWeb";
        private const string _tokenAppHostKey = "SPAppTokenHost";

        private const string _tokenUserWebKey = "SPUserTokenWeb";
        private const string _tokenUserHostKey = "SPUserTokenHost";

        private readonly LowTrustSharePointOptions _options;
        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly IAcsClient _acsClient;

        public LowTrustSharePointContext(
            IHttpContextAccessor httpContextAccessor,
            HttpClient httpClient,
            IAcsClient acsClient,
            IOptions<LowTrustSharePointOptions> options) : base(httpContextAccessor, httpClient)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(LowTrustSharePointOptions));
            _acsClient = acsClient;

            _tokenHandler = new JwtSecurityTokenHandler();
        }

        public ValueTask<SharePointTokenResult> GetAppOnlyAccessTokenForSPAppWeb()
            => AppSessionTokenHandler(_tokenAppWebKey, SPAppWebUrl);

        public ValueTask<SharePointTokenResult> GetAppOnlyAccessTokenForSPHost()
            => AppSessionTokenHandler(_tokenAppHostKey, SPHostUrl);

        public ValueTask<SharePointUserTokenResult> GetUserAccessTokenForSPAppWeb()
            => UserSessionTokenHandler(_tokenUserWebKey, SPAppWebUrl);

        public ValueTask<SharePointUserTokenResult> GetUserAccessTokenForSPHost()
            => UserSessionTokenHandler(_tokenUserHostKey, SPHostUrl);


        private async ValueTask<AcsTokenResponse> GetAppOnlyToken(Uri target)
        {
            if (target == null)
            {
                return null;
            }

            var realm = await GetRealm(target);

            var resource = Utils.GetFormattedPrincipal(
                    SharePointContextConstants.SharePointPrincipal,
                    target.Authority,
                    realm);

            var clientId = Utils.GetFormattedPrincipal(
                _options.ClientId,
                _options.HostedAppHostName ?? GetRequestUri().Authority,
                realm);

            return await _acsClient
                .GetAppOnlyAccessToken(
                    clientId,
                    _options.ClientSecret,
                    resource,
                    realm);
        }

        private async ValueTask<AcsTokenResponse> GetUserAccessToken(Uri target)
        {
            if (target == null)
            {
                return null;
            }

            var realm = await GetRealm(target);

            var resource = Utils.GetFormattedPrincipal(
                    SharePointContextConstants.SharePointPrincipal,
                    target.Authority,
                    realm);

            var clientId = Utils.GetFormattedPrincipal(
                _options.ClientId,
                _options.HostedAppHostName ?? GetRequestUri().Authority,
                realm);

            var sharePointContext = GetSharePointContext();

            return await _acsClient.GetUserAccessTokenWithRefreshToken(
                clientId,
                _options.ClientSecret,
                sharePointContext.RefreshToken,
                resource,
                realm);
        }

        private async ValueTask<SharePointTokenResult> AppSessionTokenHandler(string key, Uri target)
        {
            var tokenResult = GetSessionValueOrDefault<SharePointTokenResult>(key);
            if (tokenResult == null || tokenResult.Expires.AddMinutes(-1) <= DateTime.UtcNow)
            {
                var tokenResponse = await GetAppOnlyToken(target);
                tokenResult = new SharePointTokenResult(tokenResponse.AccessToken, tokenResponse.ExpiresOn);

                SetSessionValue(key, tokenResult);
            }

            return tokenResult;
        }

        private async ValueTask<SharePointUserTokenResult> UserSessionTokenHandler(string key, Uri target)
        {
            var tokenResult = GetSessionValueOrDefault<SharePointUserTokenResult>(key);
            if (tokenResult == null || tokenResult.Expires.AddMinutes(-1) <= DateTime.UtcNow)
            {
                var tokenResponse = await GetUserAccessToken(target);

                if (tokenResponse == null)
                {
                    return null;
                }

                var user = await GetSharePointContextUser(target, tokenResponse.AccessToken);

                tokenResult = new SharePointUserTokenResult(tokenResponse.AccessToken, tokenResponse.ExpiresOn, user);

                SetSessionValue(key, tokenResult);
            }

            return tokenResult;
        }

        private async Task<string> GetRealm(Uri target)
         => await GetRealmFromTargetUrl(target) ?? _options.Realm;

        private SharePointContextToken GetSharePointContext()
        {
            var ctx = GetSessionValueOrDefault<SharePointContextToken>(_contextTokenKey);
            if (ctx == null)
            {
                ctx = ReadAndValidateContextToken();
                SetSessionValue(_contextTokenKey, ctx);
            }
            return ctx;
        }

        private SharePointContextToken ReadAndValidateContextToken()
        {
            var spToken = GetSharePointTokenFromRequest();
            ValidateContextToken(spToken);

            return spToken;
        }

        private void ValidateContextToken(SharePointContextToken spToken)
        {
            var audience = spToken.Audiences.First();
            var realm = _options.Realm ?? spToken.Realm;

            var principal = Utils.GetFormattedPrincipal(_options.ClientId, GetRequestUri().Authority, realm);

            if (!audience.Equals(principal, StringComparison.OrdinalIgnoreCase))
            {
                throw new SecurityTokenInvalidAudienceException($"{audience} is not the intended audience {principal}");
            }
        }

        private SharePointContextToken GetSharePointTokenFromRequest()
        {
            var spToken = GetSharePointTokenValue();

            var securityKeys = new List<SymmetricSecurityKey>
            {
                new SymmetricSecurityKey(Convert.FromBase64String(_options.ClientSecret))
            };

            if (!string.IsNullOrEmpty(_options.SecondaryClientSecret))
            {
                securityKeys.Add(new SymmetricSecurityKey(Convert.FromBase64String(_options.SecondaryClientSecret)));
            }

            _tokenHandler.ValidateToken(
                spToken,
                new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false, // validated below
                    IssuerSigningKeys = securityKeys // validate the signature
                },
                out var securityToken);

            return new SharePointContextToken(securityToken);
        }

        private string GetSharePointTokenValue()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            string[] paramNames = { "AppContext", "AppContextToken", "AccessToken", "SPAppToken" };

            foreach (string paramName in paramNames)
            {
                if (!string.IsNullOrEmpty(request.Form[paramName]))
                {
                    return request.Form[paramName];
                }
                if (request.Query.TryGetValue(paramName, out var value))
                {
                    return value;
                }
            }

            return null;
        }

        private Uri GetRequestUri()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var uriBuilder = new UriBuilder
            {
                Scheme = request.Scheme,
                Host = request.Host.Host,
                Port = request.Host.Port.GetValueOrDefault(request.Scheme == "https" ? 443 : 80),
                Path = request.Path.ToString(),
                Query = request.QueryString.ToString()
            };
            return uriBuilder.Uri;
        }
    }
}
