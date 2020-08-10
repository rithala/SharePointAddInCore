using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using SharePointAddInCore.Core.SharePointContext;

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SharePointAddInCore.HighTrust
{
    internal class HighTrustSharePointContext : SharePointContextBase, ISharePointContext
    {
        private readonly HighTrustSharePointOptions _options;

        public HighTrustSharePointContext(
            IHttpContextAccessor httpContextAccessor,
            HttpClient httpClient,
            IOptions<HighTrustSharePointOptions> options) : base(httpContextAccessor)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(HighTrustSharePointOptions));
        }

        public ValueTask<SharePointTokenResult> GetAppOnlyAccessToken(Uri sharePointSiteUri)
        {
            throw new NotImplementedException();
        }

        public ValueTask<SharePointTokenResult> GetAppOnlyAccessTokenForSPAppWeb()
        {
            throw new NotImplementedException();
        }

        public ValueTask<SharePointTokenResult> GetAppOnlyAccessTokenForSPHost()
        {
            throw new NotImplementedException();
        }

        public ValueTask<SharePointUserTokenResult> GetUserAccessTokenForSPAppWeb()
        {
            throw new NotImplementedException();
        }

        public ValueTask<SharePointUserTokenResult> GetUserAccessTokenForSPHost()
        {
            throw new NotImplementedException();
        }
    }
}
