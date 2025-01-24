using Asp.Versioning.Builder;
using Bureau.UI.API.Configurations;
using Bureau.UI.API.Features.Recipes.V1.Configurations;
using Bureau.UI.API.Features.Recipes.V2.Configurations;
using Bureau.UI.API.Features.Recipes.V3.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Bureau.UI.API.Features.Recipes.Configurations
{
    internal static partial class BureauRecipeAPIWebAppConfiguration
    {
        internal static void MapRecipes(this RouteGroupBuilder apiGroupBuilder, ApiVersionSet versionSet)
        {
            RouteGroupBuilder recipesGroup = apiGroupBuilder.MapGroup(BureauAPIRouteNames.RecipesGroup);

            recipesGroup.MapDelete("{id}", RecipesMethods.DeleteRecipe)
                .WithName($"{BureauAPIRouteNames.DeleteRecipes}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version1)
                .MapToApiVersion(BureauAPIVersion.Version2)
                .MapToApiVersion(BureauAPIVersion.Version3);

            apiGroupBuilder.MapRecipesV1(versionSet);
            apiGroupBuilder.MapRecipesV2(versionSet);
            apiGroupBuilder.MapRecipesV3(versionSet);
        }
    }
}
