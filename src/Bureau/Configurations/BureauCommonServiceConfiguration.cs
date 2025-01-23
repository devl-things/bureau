using Bureau.Core.Services;
using Bureau.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Bureau.Configurations
{
    public static class BureauCommonServiceConfiguration
    {
        public static IServiceCollection AddBureauCommon(this IServiceCollection services)
        {
            services.AddSingleton(TimeProvider.System);
            services.AddSingleton<IPaginationValidationService, PaginationValidationService>();
            return services;
        }
    }
}
