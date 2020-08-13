using Newtonsoft.Json;

using SharePointAddInCore.Core.SharePointClient.Models;

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SharePointAddInCore.Core.SharePointClient
{
    internal class RestSharePointClient : ISharePointClient
    {
        private readonly HttpClient _httpClient;

        public RestSharePointClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<SharePointContextUser> GetSharePointContextUser(Uri target, string accessToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(target, "_api/web/currentUser")))
            {
                request.Headers.Add("Authorization", "Bearer " + accessToken);
                request.Headers.Add("Accept", "application/json;odata=verbose");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();

                var odataResponse = JsonConvert.DeserializeObject<ODataResponse<SharePointContextUser>>(jsonResponse);

                return odataResponse.Data;
            }
        }

        public async Task<string> GetAuthenticationRealm(Uri target)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(target, "_vti_bin/client.svc")))
            {
                request.Headers.Add("Authorization", "Bearer ");

                var response = await _httpClient.SendAsync(request);

                if (response.Headers.WwwAuthenticate.Count == 0)
                {
                    return null;
                }

                var bearerResponseHeader = response.Headers.WwwAuthenticate.First().Parameter;

                const string realm = "realm=\"";
                var realmIndex = bearerResponseHeader.IndexOf(realm, StringComparison.Ordinal);

                if (realmIndex < 0)
                {
                    return null;
                }

                var realmStartIndex = realmIndex + realm.Length;

                if (bearerResponseHeader.Length >= realmStartIndex + 36)
                {
                    string targetRealm = bearerResponseHeader.Substring(realmStartIndex, 36);

                    Guid realmGuid;

                    if (Guid.TryParse(targetRealm, out realmGuid))
                    {
                        return targetRealm;
                    }
                }

                return null;
            }
        }
    }
}
