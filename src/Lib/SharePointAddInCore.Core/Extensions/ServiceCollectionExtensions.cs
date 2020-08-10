using Microsoft.Extensions.DependencyInjection;

using SharePointAddInCore.Core.SharePointClient;

namespace SharePointAddInCore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSharePointCoreServices(this IServiceCollection services)
        {
            services.AddHttpClient<ISharePointClient, RestSharePointClient>();

            return services;
        }
    }
}
