using Newtonsoft.Json;

using System;

namespace SharePointAddInCore.Core.SharePointContext
{
    public class SharePointTokenResult
    {
        public string AccessToken { get; }
        public DateTime Expires { get; }

        [JsonConstructor]
        public SharePointTokenResult(string accessToken, DateTime expires)
        {
            AccessToken = accessToken;
            Expires = expires;
        }
    }
}
