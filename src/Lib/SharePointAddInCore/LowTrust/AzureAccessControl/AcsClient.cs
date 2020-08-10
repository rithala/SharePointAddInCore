using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SharePointAddInCore.LowTrust.AzureAccessControl
{
    internal interface IAcsClient
    {
        Task<AcsTokenResponse> GetAppOnlyAccessToken(string clientId, string clientSecret, string resource, string realm);
        Task<AcsTokenResponse> GetUserAccessTokenWithRefreshToken(string clientId, string clientSecret, string refreshToken, string resource, string realm);
        Task<AcsTokenResponse> GetUserAccessTokenWithAuthorizationCode(string clientId, string clientSecret, string code, string redirectUri, string resource, string realm);
    }
    internal class AcsClient : IAcsClient
    {
        private const string _acsPrincipalName = "00000001-0000-0000-c000-000000000000";
        private static string _globalEndPointPrefix = "accounts";
        private static string _acsHostUrl = "accesscontrol.windows.net";
        private const string _acsMetadataEndPointRelativeUrl = "metadata/json/1";

        private readonly HttpClient _httpClient;

        public AcsClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<AcsTokenResponse> GetAppOnlyAccessToken(string clientId, string clientSecret, string resource, string realm)
            => GetAccessToken(
                realm,
                new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "resource", resource }
                });

        public Task<AcsTokenResponse> GetUserAccessTokenWithRefreshToken(string clientId, string clientSecret, string refreshToken, string resource, string realm)
            => GetAccessToken(
                realm,
                new Dictionary<string, string>
                {
                    { "grant_type", "refresh_token" },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "refresh_token", refreshToken },
                    { "resource", resource }
                });

        public Task<AcsTokenResponse> GetUserAccessTokenWithAuthorizationCode(string clientId, string clientSecret, string code, string redirectUri, string resource, string realm)
            => GetAccessToken(
                realm,
                new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "code", code },
                    { "redirect_uri", redirectUri },
                    { "resource", resource }
                });

        private async Task<AcsTokenResponse> GetAccessToken(string realm, IDictionary<string, string> requestData)
        {
            var metadata = await GetMetadataDocument(realm);

            var response = await PostData(metadata.GetStsUrl(), requestData);

            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<AcsTokenResponse>(jsonResponse);
        }

        private async Task<JsonMetadataDocument> GetMetadataDocument(string realm)
        {
            var acsMetadataEndpointUrlWithRealm = string.Format(CultureInfo.InvariantCulture, "{0}?realm={1}",
                                                                       GetAcsMetadataEndpointUrl(),
                                                                       realm);

            var response = await _httpClient.GetAsync(acsMetadataEndpointUrlWithRealm);
            response.EnsureSuccessStatusCode();

            var document = JsonConvert.DeserializeObject<JsonMetadataDocument>(
                await response.Content.ReadAsStringAsync());

            if (document == null)
            {
                throw new Exception("No metadata document found at the global endpoint " + acsMetadataEndpointUrlWithRealm);
            }

            return document;
        }

        private async Task<HttpResponseMessage> PostData(Uri uri, IDictionary<string, string> data)
            => await _httpClient.PostAsync(uri, new FormUrlEncodedContent(data));

        private static string GetAcsGlobalEndpoint()
            => string.Format(CultureInfo.InvariantCulture, "https://{0}.{1}/", _globalEndPointPrefix, _acsHostUrl);

        private static string GetAcsMetadataEndpointUrl()
            => Path.Combine(GetAcsGlobalEndpoint(), _acsMetadataEndPointRelativeUrl);

        private static string GetAcsPrincipalName(string realm)
            => Utils.GetFormattedPrincipal(_acsPrincipalName, new Uri(GetAcsGlobalEndpoint()).Host, realm);
    }
}
