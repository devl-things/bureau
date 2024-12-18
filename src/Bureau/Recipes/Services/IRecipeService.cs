using Bureau.Core;
using Bureau.Recipes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Recipes.Services
{
    internal interface IRecipeService
    {
        Task<Result<RecipeAggregate>> GetRecipeAggregateAsync(string id, CancellationToken cancellationToken);
    }
}
