using Bureau.Recipes.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace Bureau.UI.API.V1.Configurations
{
    internal static class BureauAPIServiceConfiguration
    {
        internal static void AddRecipes(this IServiceCollection services) 
        {
            services.AddBureauRecipes();
        }
    }
}
