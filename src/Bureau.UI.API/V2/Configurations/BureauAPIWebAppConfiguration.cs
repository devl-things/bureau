using Bureau.UI.API.V2.Methods;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Asp.Versioning.Conventions;
using Asp.Versioning.Builder;
using Bureau.UI.API.Configurations;


namespace Bureau.UI.API.V2.Configurations
{
    public static partial class BureauAPIWebAppConfiguration
    {
        internal static void MapRecipesV2(this RouteGroupBuilder apiGroupBuilder, ApiVersionSet versionSet)
        {
            
            RouteGroupBuilder recipesGroup = apiGroupBuilder.MapGroup(BureauAPIRouteNames.RecipesGroup);

            recipesGroup.MapGet("", RecipesMethods.GetRecipes)
                .WithName($"{BureauAPIRouteNames.GetRecipes}-{BureauAPIVersion.Version2}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version2);
            recipesGroup.MapGet("{id}", RecipesMethods.GetRecipeById)
                .WithName($"{BureauAPIRouteNames.GetRecipeById}-{BureauAPIVersion.Version2}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version2);

            recipesGroup.MapPost("", RecipesMethods.CreateRecipe)
                .WithName($"{BureauAPIRouteNames.CreateRecipes}-{BureauAPIVersion.Version2}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version2);

            recipesGroup.MapPut("{id}", RecipesMethods.UpdateRecipe)
                .WithName($"{BureauAPIRouteNames.UpdateRecipes}-{BureauAPIVersion.Version2}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version2);

            recipesGroup.MapDelete("{id}", RecipesMethods.DeleteRecipe)
                .WithName($"{BureauAPIRouteNames.DeleteRecipes}-{BureauAPIVersion.Version2}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version2);
        }
    }
}
