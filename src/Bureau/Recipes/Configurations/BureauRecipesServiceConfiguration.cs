using Bureau.Recipes.Handlers;
using Bureau.Recipes.Managers;
using Microsoft.Extensions.DependencyInjection;

namespace Bureau.Recipes.Configurations
{
    public static class BureauRecipesServiceConfiguration
    {
        public static IServiceCollection AddBureauRecipes(this IServiceCollection services) 
        {
            services.AddScoped<IRecipeCommandHandler, RecipeCommandHandler>();
            services.AddScoped<IInternalRecipeQueryHandler, RecipeQueryHandler>();
            services.AddScoped<IRecipeQueryHandler, RecipeQueryHandler>();
            services.AddScoped<IRecipeManager, RecipeManager>();

            return services;
        }
    }
}
