using System;

namespace SharePointAddInCore.Core.SharePointContext
{
    public interface ISharePointContextProps
    {
        Uri SPHostUrl { get; }
        Uri SPAppWebUrl { get; }
        string SPLanguage { get; }
        string SPClientTag { get; }
        string SPProductNumber { get; }
    }
}
