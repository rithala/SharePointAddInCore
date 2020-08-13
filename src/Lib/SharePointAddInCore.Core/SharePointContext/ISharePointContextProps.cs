using System;

namespace SharePointAddInCore.Core.SharePointContext
{
    /// <summary>
    /// Basic information about the SharePoint connection.
    /// </summary>
    public interface ISharePointContextProps
    {
        Uri SPHostUrl { get; }
        Uri SPAppWebUrl { get; }
        string SPLanguage { get; }
        string SPClientTag { get; }
        string SPProductNumber { get; }
    }
}
