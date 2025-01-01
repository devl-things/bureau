using Bureau.Recipes.Models;
using Bureau.UI.API.V1.Models.Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.UI.API.V1.Mappers
{
    internal static class RecipeRequestModelMapper
    {
        internal static RecipeDto ToDto(this RecipeRequestModel recipe)
        {
            return new RecipeDto()
            {
                Name = recipe.Name,
                SubGroups = new List<RecipeSubGroupDto>()
                {
                    new RecipeSubGroupDto(string.Empty)
                    {
                        Name = recipe.Name,
                        Ingredients = recipe.Ingredients,
                        Instructions = recipe.Instructions
                    }
                },
                PreparationTime = recipe.PreparationTime,
                Servings = recipe.Servings,
                CreatedAt = DateTime.Now.ToUniversalTime(),
                UpdatedAt = DateTime.Now.ToUniversalTime()
            };
        }

        internal static RecipeDto ToDto(this RecipeRequestModel recipe, string id)
        {
            RecipeDto result = recipe.ToDto();
            result.Id = id;
            return result;
        }
    }
}
