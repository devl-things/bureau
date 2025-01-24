using Bureau.Calendar.Factories;
using Bureau.Calendar.Models;
using Bureau.Factories;
using Bureau.Handlers;
using Bureau.Managers;
using Bureau.Providers;
using Bureau.Recipes.Factories;
using Bureau.Recipes.Models;
using Bureau.Recipes.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Bureau.Recipes.Configurations
{
    public static class BureauRecipesServiceConfiguration
    {
        public static IServiceCollection AddBureauRecipes(this IServiceCollection services)
        {
            services.AddScoped<IDtoFactory<RecipeDto>, RecipeDtoFactory>();
            services.AddScoped<IInternalRecipeQueryHandler, RecipeProvider>();
            services.AddScoped<ICommandHandler<RecipeDto>, RecipeCommandHandler>();
            services.AddScoped<IDtoProvider<RecipeDto>, RecipeProvider>();
            services.AddScoped<IDtoManager<RecipeDto>, RecipeManager>();

            return services;
        }
    }
}
