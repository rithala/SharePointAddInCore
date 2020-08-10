using Microsoft.IdentityModel.Tokens;

using SharePointAddInCore.Core;

using System.Security.Cryptography.X509Certificates;

namespace SharePointAddInCore.HighTrust
{
    /// <summary>
    /// Options to configure the high trust SharePoint Add-in
    /// </summary>
    public class HighTrustSharePointOptions : CommonSharePointOptions
    {
        private string _issuerId;

        /// <summary>
        /// Issuer id
        /// </summary>
        public string IssuerId
        {
            get { return _issuerId ?? ClientId; }
            set { _issuerId = value; }
        }
        public string ClientSigningCertificatePath { get; set; }
        public string ClientSigningCertificatePassword { get; set; }

        internal X509Certificate2 ClientCertificate => (string.IsNullOrEmpty(ClientSigningCertificatePath) || string.IsNullOrEmpty(ClientSigningCertificatePassword))
            ? null
            : new X509Certificate2(ClientSigningCertificatePath, ClientSigningCertificatePassword);

        internal X509SigningCredentials SigningCredentials => (ClientCertificate == null)
            ? null
            : new X509SigningCredentials(ClientCertificate, SecurityAlgorithms.RsaSha256);
    }
}
