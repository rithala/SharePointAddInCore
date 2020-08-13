using Microsoft.Extensions.DependencyInjection;

using SharePointAddInCore.Core.SharePointContext;
using SharePointAddInCore.HighTrust;

using System;

namespace SharePointAddInCore
{
    /// <summary>
    /// Extension methods adding the library features to ASP.NET Core Apps.
    /// </summary>
    public static class HighTrustExtensions
    {
        /// <summary>
        /// Adds SharePoint high trust add-in (using S2S) services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="configure">The action used to configure the options.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddHighTrustAddIn(this IServiceCollection services, Action<HighTrustSharePointOptions> configure)
        {
            services.AddSharePointCoreServices();

            services.Configure(configure);

            services.AddScoped<ITokenIssuer, TokenIssuer>();
            services.AddScoped<ISharePointContext, HighTrustSharePointContext>();

            return services;
        }
    }
}
