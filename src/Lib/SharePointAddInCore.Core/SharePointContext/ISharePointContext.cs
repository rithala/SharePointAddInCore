using System;
using System.Threading.Tasks;

namespace SharePointAddInCore.Core.SharePointContext
{
    /// <summary>
    /// The core service responsible for acquiring access tokens and providing basic information about the SharePoint connection.
    /// </summary>
    public interface ISharePointContext : ISharePointContextProps
    {
        /// <summary>
        /// Get the user access token for the SPHost site.
        /// </summary>
        /// <returns>The <see cref="SharePointUserTokenResult"/> result with the access token and a basic user information.</returns>
        ValueTask<SharePointUserTokenResult> GetUserAccessTokenForSPHost();

        /// <summary>
        /// Get the user access token for the SPAppWeb site.
        /// </summary>
        /// <returns>The <see cref="SharePointUserTokenResult"/> result with the access token and a basic user information.</returns>
        ValueTask<SharePointUserTokenResult> GetUserAccessTokenForSPAppWeb();

        /// <summary>
        /// Get the application access token for the SPHost site.
        /// </summary>
        /// <returns>The <see cref="SharePointTokenResult"/> result with the access token.</returns>
        ValueTask<SharePointTokenResult> GetAppOnlyAccessTokenForSPHost();

        /// <summary>
        /// Get the application access token for the SPAppWeb site.
        /// </summary>
        /// <returns>The <see cref="SharePointTokenResult"/> result with the access token.</returns>
        ValueTask<SharePointTokenResult> GetAppOnlyAccessTokenForSPAppWeb();

        /// <summary>
        /// Get the application access token for the provided SharePoint site.
        /// </summary>
        /// <param name="sharePointSiteUri">The SharePoint site for which the access token will be acquired.</param>
        /// <returns>The <see cref="SharePointTokenResult"/> result with the access token.</returns>
        ValueTask<SharePointTokenResult> GetAppOnlyAccessToken(Uri sharePointSiteUri);
    }
}
