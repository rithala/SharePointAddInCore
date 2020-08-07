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

        public ValueTask<string> GetAppOnlyAccessTokenForSPAppWeb()
            => SessionTokenHandler(_tokenAppWebKey, () => GetAppOnlyToken(SPAppWebUrl));

        public ValueTask<string> GetAppOnlyAccessTokenForSPHost()
            => SessionTokenHandler(_tokenAppHostKey, () => GetAppOnlyToken(SPHostUrl));

        public ValueTask<string> GetUserAccessTokenForSPAppWeb()
            => SessionTokenHandler(_tokenUserWebKey, () => GetUserAccessToken(SPAppWebUrl));

        public ValueTask<string> GetUserAccessTokenForSPHost()
            => SessionTokenHandler(_tokenUserHostKey, () => GetUserAccessToken(SPHostUrl));

        public async ValueTask<AcsTokenResponse> GetAppOnlyToken(Uri target)
        {
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

        private async ValueTask<string> SessionTokenHandler(string key, Func<ValueTask<AcsTokenResponse>> getTokenFunc)
        {
            var session = GetSessionValueOrDefault<AcsTokenResponse>(key);
            if (session == null || session.ExpiresOn > DateTime.UtcNow.AddMinutes(-1))
            {
                session = await getTokenFunc.Invoke();
                SetSessionValue(key, session);
            }

            return session.AccessToken;
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
            var builder = new UriBuilder();

            var request = _httpContextAccessor.HttpContext.Request;
            builder.Scheme = request.Scheme;
            builder.Host = request.Host.Value;
            builder.Path = request.Path;
            builder.Query = request.QueryString.ToUriComponent();
            return builder.Uri;
        }
    }
}
