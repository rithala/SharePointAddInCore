using Newtonsoft.Json;

using SharePointAddInCore.Core.SharePointClient.Models;

using System;

namespace SharePointAddInCore.Core.SharePointContext
{
    public class SharePointUserTokenResult : SharePointTokenResult
    {
        public SharePointContextUser User { get; }

        [JsonConstructor]
        public SharePointUserTokenResult(
            string accessToken,
            DateTime expires,
            SharePointContextUser user) : base(accessToken, expires)
        {
            User = user;
        }
    }
}
