using Bureau.Core;
using Bureau.Recipes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Recipes.Managers
{
    public interface IRecipeManager
    {
        Task<Result<RecipeModel>> GetRecipeAsync(string id, CancellationToken cancellationToken = default);
    }
}
