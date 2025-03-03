﻿using Bureau.Recipes.Models;
using Bureau.UI.API.Features.Recipes.V1.Models;

namespace Bureau.UI.API.Features.Recipes.V1.Mappers
{
    internal static class RecipeResponseModelMapper
    {
        internal static RecipeResponseModel ToResponseModel(this RecipeDto recipe)
        {
            return new RecipeResponseModel()
            {
                Id = recipe.Id,
                Name = recipe.Name,
                Ingredients = recipe.SubGroups.FirstOrDefault()?.Ingredients.Select(x => x.Ingredient).ToList() ?? new List<string>(),
                Instructions = recipe.SubGroups.FirstOrDefault()?.Instructions ?? null,
                PreparationTime = recipe.PreparationTime,
                Servings = recipe.Servings,
                CreatedAt = recipe.CreatedAt,
                UpdatedAt = recipe.UpdatedAt
            };
        }
    }
}
