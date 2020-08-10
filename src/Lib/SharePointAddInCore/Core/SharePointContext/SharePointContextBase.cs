using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

using Newtonsoft.Json;

using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SharePointAddInCore.Core.SharePointContext
{
    internal abstract class SharePointContextBase : ISharePointContextProps
    {
        private const string _cacheKey = "SPContextProps";

        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly HttpClient _httpClient;

        public Uri SPHostUrl { get; private set; }

        public Uri SPAppWebUrl { get; private set; }

        public string SPLanguage { get; private set; }

        public string SPClientTag { get; private set; }

        public string SPProductNumber { get; private set; }

        public SharePointContextBase(IHttpContextAccessor httpContextAccessor, HttpClient httpClient)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;

            if (!TryLoadPropsFromSession())
            {
                LoadPropsFromHttpContext();
            }
        }

        protected async ValueTask<string> GetRealmFromTargetUrl(Uri url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url, "_vti_bin/client.svc")))
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

        protected async Task<SharePointContextUser> GetSharePointContextUser(Uri target, string accessToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(target, "_api/web/currentUser")))
            {
                request.Headers.Add("Authorization", "Bearer " + accessToken);
                request.Headers.Add("Accept", "application/json");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<SharePointContextUser>(jsonResponse);
            }
        }

        protected T GetSessionValueOrDefault<T>(string key)
        {
            if (_httpContextAccessor.HttpContext.Session.TryGetValue(key, out var bytes))
            {
                return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes));
            }

            return default;
        }

        protected void SetSessionValue<T>(string key, T value)
        {
            _httpContextAccessor.HttpContext.Session.Set(key, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)));
        }

        protected void RemoveSessionValue(string key)
        {
            _httpContextAccessor.HttpContext.Session.Remove(key);
        }


        private bool TryLoadPropsFromSession()
        {
            var props = GetSessionValueOrDefault<SharePointContextProps>(_cacheKey);

            if (props == null)
            {
                return false;
            }

            SPHostUrl = props.SPHostUrl;
            SPAppWebUrl = props.SPAppWebUrl;
            SPLanguage = props.SPLanguage;
            SPClientTag = props.SPClientTag;
            SPProductNumber = props.SPProductNumber;

            return true;
        }

        private void LoadPropsFromHttpContext()
        {
            var req = _httpContextAccessor.HttpContext?.Request ?? throw new ArgumentNullException("HttpContext.Request");

            if (req.Query.TryGetValue(SharePointContextConstants.SPHostUrlKey, out var param))
            {
                SPHostUrl = new Uri(param.EnsureTrailingSlash());
            }

            if (req.Query.TryGetValue(SharePointContextConstants.SPAppWebUrlKey, out param))
            {
                SPAppWebUrl = new Uri(param.EnsureTrailingSlash());
            }

            if (req.Query.TryGetValue(SharePointContextConstants.SPLanguageKey, out param))
            {
                SPLanguage = param;
            }

            if (req.Query.TryGetValue(SharePointContextConstants.SPClientTagKey, out param))
            {
                SPClientTag = param;
            }

            if (req.Query.TryGetValue(SharePointContextConstants.SPProductNumberKey, out param))
            {
                SPProductNumber = param;
            }

            SetSessionValue(
                _cacheKey,
                new SharePointContextProps(
                    SPHostUrl,
                    SPAppWebUrl,
                    SPLanguage,
                    SPClientTag,
                    SPProductNumber));
        }

        private class SharePointContextProps : ISharePointContextProps
        {
            public Uri SPHostUrl { get; }

            public Uri SPAppWebUrl { get; }

            public string SPLanguage { get; }

            public string SPClientTag { get; }

            public string SPProductNumber { get; }

            [JsonConstructor]
            public SharePointContextProps(
                Uri sPHostUrl,
                Uri sPAppWebUrl,
                string sPLanguage,
                string sPClientTag,
                string sPProductNumber)
            {
                SPHostUrl = sPHostUrl;
                SPAppWebUrl = sPAppWebUrl;
                SPLanguage = sPLanguage;
                SPClientTag = sPClientTag;
                SPProductNumber = sPProductNumber;
            }
        }
    }

    internal static class UrlStringExtensions
    {
        public static string EnsureTrailingSlash(this StringValues paramValue)
            => paramValue.ToString().EndsWith("/")
                ? paramValue.ToString()
                : $"{paramValue}/";
    }
}
