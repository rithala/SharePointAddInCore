using SharePointAddInCore.Core;

namespace SharePointAddInCore.LowTrust
{
    /// <summary>
    /// Options to configure the low trust SharePoint Add-in
    /// </summary>
    public class LowTrustSharePointOptions : CommonSharePointOptions
    {
        public string ClientSecret { get; set; }
        public string SecondaryClientSecret { get; set; }
        public string Realm { get; set; }
        public string HostedAppHostName { get; set; }
    }
}
