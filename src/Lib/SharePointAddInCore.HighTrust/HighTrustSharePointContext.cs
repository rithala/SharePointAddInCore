using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using SharePointAddInCore.Core.SharePointClient;
using SharePointAddInCore.Core.SharePointContext;

using System;
using System.Security.Principal;
using System.Threading.Tasks;

namespace SharePointAddInCore.HighTrust
{
    internal class HighTrustSharePointContext : SharePointContextBase, ISharePointContext
    {
        private const string _tokenAppWebKey = "SPAppTokenWeb";
        private const string _tokenAppHostKey = "SPAppTokenHost";

        private const string _tokenUserWebKey = "SPUserTokenWeb";
        private const string _tokenUserHostKey = "SPUserTokenHost";

        private readonly ISharePointClient _sharePointClient;
        private readonly ITokenIssuer _tokenIssuer;
        private readonly HighTrustSharePointOptions _options;

        public HighTrustSharePointContext(
            IHttpContextAccessor httpContextAccessor,
            ISharePointClient sharePointClient,
            ITokenIssuer tokenIssuer,
            IOptions<HighTrustSharePointOptions> options) : base(httpContextAccessor)
        {
            _sharePointClient = sharePointClient;
            _tokenIssuer = tokenIssuer;
            _options = options.Value ?? throw new ArgumentNullException(nameof(HighTrustSharePointOptions));
        }

        public async ValueTask<SharePointTokenResult> GetAppOnlyAccessToken(Uri sharePointSiteUri)
            => await GetS2SAccessTokenWithWindowsIdentity(sharePointSiteUri, null);

        public ValueTask<SharePointTokenResult> GetAppOnlyAccessTokenForSPAppWeb()
            => AppSessionTokenHandler(_tokenAppWebKey, SPAppWebUrl);

        public ValueTask<SharePointTokenResult> GetAppOnlyAccessTokenForSPHost()
            => AppSessionTokenHandler(_tokenAppHostKey, SPHostUrl);

        public ValueTask<SharePointUserTokenResult> GetUserAccessTokenForSPAppWeb()
            => UserSessionTokenHandler(_tokenUserWebKey, SPAppWebUrl);

        public ValueTask<SharePointUserTokenResult> GetUserAccessTokenForSPHost()
            => UserSessionTokenHandler(_tokenUserHostKey, SPHostUrl);

        private async ValueTask<SharePointTokenResult> AppSessionTokenHandler(string key, Uri target)
        {
            var tokenResult = GetSessionValueOrDefault<SharePointTokenResult>(key);
            if (tokenResult == null || tokenResult.Expires.AddMinutes(-1) <= DateTime.UtcNow)
            {
                tokenResult = await GetS2SAccessTokenWithWindowsIdentity(target, null);

                SetSessionValue(key, tokenResult);
            }

            return tokenResult;
        }

        private async ValueTask<SharePointUserTokenResult> UserSessionTokenHandler(string key, Uri target)
        {
            var tokenResult = GetSessionValueOrDefault<SharePointUserTokenResult>(key);
            if (tokenResult == null || tokenResult.Expires.AddMinutes(-1) <= DateTime.UtcNow)
            {
                var tokenResponse = await GetS2SAccessTokenWithWindowsIdentity(target, GetWindowsIdentity());

                var user = await _sharePointClient.GetSharePointContextUser(target, tokenResponse.AccessToken);
                tokenResult = new SharePointUserTokenResult(tokenResponse.AccessToken, tokenResponse.Expires, user);

                SetSessionValue(key, tokenResult);
            }

            return tokenResult;
        }

        private async Task<SharePointTokenResult> GetS2SAccessTokenWithWindowsIdentity(
            Uri targetApplicationUri,
            WindowsIdentity identity)
        {
            var realm = await GetRealm(targetApplicationUri);

            return _tokenIssuer.GetS2SAccessTokenWithWindowsIdentity(targetApplicationUri.Authority, realm, identity);
        }

        private async ValueTask<string> GetRealm(Uri target)
        {
            return _options.Realm ?? await _sharePointClient.GetAuthenticationRealm(target);
        }

        private WindowsIdentity GetWindowsIdentity()
        {
            var identity = (WindowsIdentity)_httpContextAccessor.HttpContext?.User?.Identity;
            if (identity == null || !identity.IsAuthenticated || identity.IsGuest || identity.User == null)
            {
                return null;
            }

            return identity;
        }
    }
}
