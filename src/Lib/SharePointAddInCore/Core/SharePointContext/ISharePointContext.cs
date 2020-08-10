using System;
using System.Threading.Tasks;

namespace SharePointAddInCore.Core.SharePointContext
{
    public interface ISharePointContext : ISharePointContextProps
    {
        ValueTask<SharePointUserTokenResult> GetUserAccessTokenForSPHost();
        ValueTask<SharePointUserTokenResult> GetUserAccessTokenForSPAppWeb();
        ValueTask<SharePointTokenResult> GetAppOnlyAccessTokenForSPHost();
        ValueTask<SharePointTokenResult> GetAppOnlyAccessTokenForSPAppWeb();
        ValueTask<SharePointTokenResult> GetAppOnlyAccessToken(Uri sharePointSiteUri);
    }
}
