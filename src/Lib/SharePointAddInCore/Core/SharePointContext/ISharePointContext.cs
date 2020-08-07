using System;
using System.Threading.Tasks;

namespace SharePointAddInCore.Core.SharePointContext
{
    public interface ISharePointContext : ISharePointContextProps
    {
        ValueTask<string> GetUserAccessTokenForSPHost();
        ValueTask<string> GetUserAccessTokenForSPAppWeb();
        ValueTask<string> GetAppOnlyAccessTokenForSPHost();
        ValueTask<string> GetAppOnlyAccessTokenForSPAppWeb();
    }
}
