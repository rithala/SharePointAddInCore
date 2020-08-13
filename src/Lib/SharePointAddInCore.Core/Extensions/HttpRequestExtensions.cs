using Microsoft.AspNetCore.Http;

using System;

namespace SharePointAddInCore.Core.Extensions
{
    internal static class HttpRequestExtensions
    {
        public static Uri GetUri(this HttpRequest request) =>
            new UriBuilder
            {
                Scheme = request.Scheme,
                Host = request.Host.Host,
                Port = request.Host.Port.GetValueOrDefault(request.Scheme == "https" ? 443 : 80),
                Path = request.Path.ToString(),
                Query = request.QueryString.ToString()
            }.Uri;
    }
}
