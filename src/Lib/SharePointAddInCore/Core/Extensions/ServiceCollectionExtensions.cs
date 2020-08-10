using Microsoft.Extensions.DependencyInjection;

using SharePointAddInCore.Core.Authentication;

using System;

namespace SharePointAddInCore
{
    public static class ServiceCollectionExtensions
    {
        internal static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            
            return services;
        }

        public static IServiceCollection AddSharePointAuthentication(this IServiceCollection services, Action<SharePointAuthenticationOptions> configureOptions = null)
        {
            services
                .AddAuthentication(SharePointAuthentication.SchemeName)
                .AddSharePointAddIn(configureOptions);

            return services;
        }
    }
}
