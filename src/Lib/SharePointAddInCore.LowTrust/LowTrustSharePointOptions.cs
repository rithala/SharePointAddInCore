using SharePointAddInCore.Core;

namespace SharePointAddInCore
{
    /// <summary>
    /// Options to configure the low trust SharePoint Add-in
    /// </summary>
    public class LowTrustSharePointOptions : CommonSharePointOptions
    {
        /// <summary>
        /// The secret generated on the ~siteurl/_layouts/15/AppRegNew.aspx page.
        /// </summary>
        public string ClientSecret { get; set; }
        /// <summary>
        /// The secondary client secret.
        /// </summary>
        public string SecondaryClientSecret { get; set; }
        /// <summary>
        /// The host name of the application. Only required for retrieving App-Only access token outside an HTTP request.
        /// </summary>
        public string AddInHostName { get; set; }
    }
}
