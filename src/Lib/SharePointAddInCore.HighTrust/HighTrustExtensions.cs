using Microsoft.Extensions.DependencyInjection;

using SharePointAddInCore.Core.SharePointContext;
using SharePointAddInCore.HighTrust;

using System;

namespace SharePointAddInCore
{
    public static class HighTrustExtensions
    {
        public static IServiceCollection AddHighTrustAddIn(this IServiceCollection services, Action<HighTrustSharePointOptions> configure = null)
        {
            services.AddSharePointCoreServices();

            if (configure != null)
            {
                services.Configure(configure);
            }

            services.AddScoped<ISharePointContext, HighTrustSharePointContext>();

            return services;
        }
    }
}
