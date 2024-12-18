using Bureau.Recipes.Managers;
using Bureau.Recipes.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Bureau.Recipes.Configurations
{
    public static class BureauRecipesServiceConfiguration
    {
        public static IServiceCollection AddBureauRecipes(this IServiceCollection services) 
        {
            services.AddScoped<IRecipeService, RecipeService>();
            services.AddScoped<IRecipeManager, RecipeManager>();

            return services;
        }
    }
}
