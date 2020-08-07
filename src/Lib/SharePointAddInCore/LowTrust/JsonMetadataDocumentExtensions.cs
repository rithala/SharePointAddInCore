using System;
using System.Linq;

namespace SharePointAddInCore.LowTrust
{
    internal static class JsonMetadataDocumentExtensions
    {
        private const string _s2SProtocol = "OAuth2";

        public static Uri GetStsUrl(this JsonMetadataDocument document)
        {
            var s2sEndpoint = document.Endpoints.SingleOrDefault(e => e.Protocol == _s2SProtocol);

            if (null != s2sEndpoint)
            {
                return new Uri(s2sEndpoint.Location);
            }

            throw new Exception("Metadata document does not contain STS endpoint url");
        }
    }
}
