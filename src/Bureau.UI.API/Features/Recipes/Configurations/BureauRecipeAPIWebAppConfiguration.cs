using Asp.Versioning.Builder;
using Bureau.UI.API.Configurations;
using Bureau.UI.API.Features.Recipes.V1.Configurations;
using Bureau.UI.API.Features.Recipes.V2.Configurations;
using Bureau.UI.API.Features.Recipes.V3.Configurations;
using Bureau.Core.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Bureau.UI.API.Features.Recipes.Configurations
{
    internal static partial class BureauRecipeAPIWebAppConfiguration
    {
        internal static void MapRecipes(this RouteGroupBuilder apiGroupBuilder, ApiVersionSet versionSet)
        {
            RouteGroupBuilder recipesGroup = apiGroupBuilder.MapGroup(BureauAPIRouteNames.RecipesGroup)
                .WithTags(BureauAPIRouteNames.RecipesGroup.ToPascalCase());

            recipesGroup.MapDelete("{id}", RecipesMethods.DeleteRecipe)
                .WithName($"{BureauAPIRouteNames.DeleteRecipes}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version1)
                .MapToApiVersion(BureauAPIVersion.Version2)
                .MapToApiVersion(BureauAPIVersion.Version3);

            recipesGroup.MapRecipesV1(versionSet);
            recipesGroup.MapRecipesV2(versionSet);
            recipesGroup.MapRecipesV3(versionSet);
        }
    }
}
