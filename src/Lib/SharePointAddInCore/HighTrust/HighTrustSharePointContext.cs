using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using SharePointAddInCore.Core;
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
            IOptions<HighTrustSharePointOptions> options) : base(httpContextAccessor, httpClient)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(HighTrustSharePointOptions));
        }

        public ValueTask<string> GetAppOnlyAccessTokenForSPAppWeb()
        {
            throw new NotImplementedException();
        }

        public ValueTask<string> GetAppOnlyAccessTokenForSPHost()
        {
            throw new NotImplementedException();
        }

        public ValueTask<string> GetUserAccessTokenForSPAppWeb()
        {
            throw new NotImplementedException();
        }

        public ValueTask<string> GetUserAccessTokenForSPHost()
        {
            throw new NotImplementedException();
        }
    }
}
