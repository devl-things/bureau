using Bureau.UI.API.V3.Methods;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Asp.Versioning.Conventions;
using Asp.Versioning.Builder;
using Bureau.UI.API.Configurations;

namespace Bureau.UI.API.V3.Configurations
{
    public static partial class BureauAPIWebAppConfiguration
    {
        internal static void MapRecipesV3(this RouteGroupBuilder apiGroupBuilder, ApiVersionSet versionSet)
        {
            
            RouteGroupBuilder recipesGroup = apiGroupBuilder.MapGroup(BureauAPIRouteNames.RecipesGroup);

            recipesGroup.MapGet("", RecipesMethods.GetRecipes)
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

            recipesGroup.MapDelete("{id}", RecipesMethods.DeleteRecipe)
                .WithName($"{BureauAPIRouteNames.DeleteRecipes}-{BureauAPIVersion.Version3}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version3);
        }
    }
}
