using System.Globalization;

namespace SharePointAddInCore.LowTrust
{
    internal static class Utils
    {
        public static string GetFormattedPrincipal(string principalName, string hostName, string realm)
        {
            if (!string.IsNullOrEmpty(hostName))
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}/{1}@{2}", principalName, hostName, realm);
            }

            return string.Format(CultureInfo.InvariantCulture, "{0}@{1}", principalName, realm);
        }
    }
}
