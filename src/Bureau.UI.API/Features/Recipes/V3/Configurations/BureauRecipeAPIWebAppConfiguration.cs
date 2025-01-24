using Asp.Versioning.Builder;
using Asp.Versioning.Conventions;
using Bureau.UI.API.Configurations;
using Bureau.UI.API.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Bureau.UI.API.Features.Recipes.V3.Configurations
{
    public static partial class BureauRecipeAPIWebAppConfiguration
    {
        internal static void MapRecipesV3(this RouteGroupBuilder apiGroupBuilder, ApiVersionSet versionSet)
        {

            RouteGroupBuilder recipesGroup = apiGroupBuilder.MapGroup(BureauAPIRouteNames.RecipesGroup);

            recipesGroup.MapGet("", RecipesMethods.GetRecipes)
                .AddEndpointFilter<PaginationValidationFilter>()
                .WithName($"{BureauAPIRouteNames.GetRecipes}-{BureauAPIVersion.Version3}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version3);
            recipesGroup.MapGet("{id}", RecipesMethods.GetRecipeById)
                .WithName($"{BureauAPIRouteNames.GetRecipeById}-{BureauAPIVersion.Version3}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version3);

            recipesGroup.MapPost("", RecipesMethods.CreateRecipe)
                .WithName($"{BureauAPIRouteNames.CreateRecipes}-{BureauAPIVersion.Version3}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version3);

            recipesGroup.MapPut("{id}", RecipesMethods.UpdateRecipe)
                .WithName($"{BureauAPIRouteNames.UpdateRecipes}-{BureauAPIVersion.Version3}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version3);
        }
    }
}
