using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

using Newtonsoft.Json;

using System;
using System.Net.Http;
using System.Text;

namespace SharePointAddInCore.Core.SharePointContext
{
    public abstract class SharePointContextBase : ISharePointContextProps
    {
        private const string _cacheKey = "SPContextProps";

        protected readonly IHttpContextAccessor _httpContextAccessor;

        public Uri SPHostUrl { get; private set; }

        public Uri SPAppWebUrl { get; private set; }

        public string SPLanguage { get; private set; }

        public string SPClientTag { get; private set; }

        public string SPProductNumber { get; private set; }

        public SharePointContextBase(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            if (!TryLoadPropsFromSession())
            {
                LoadPropsFromHttpContext();
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
