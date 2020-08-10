using Newtonsoft.Json;

using System;

namespace SharePointAddInCore.Core.SharePointContext
{
    public class SharePointUserTokenResult : SharePointTokenResult
    {
        public SharePointContextUser User { get; }

        [JsonConstructor]
        internal SharePointUserTokenResult(
            string accessToken,
            DateTime expires,
            SharePointContextUser user) : base(accessToken, expires)
        {
            User = user;
        }

    }
}
