using Bureau.Core.Services;
using Bureau.Recipes.Handlers;
using Bureau.Recipes.Managers;
using Bureau.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Configurations
{
    public static class BureauCommonServiceConfiguration
    {
        public static IServiceCollection AddBureauCommon(this IServiceCollection services)
        {
            services.AddSingleton<IPaginationValidationService, PaginationValidationService>();

            return services;
        }
    }
}
