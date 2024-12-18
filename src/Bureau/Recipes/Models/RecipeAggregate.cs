using Bureau.Core.Models;
using Bureau.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Bureau.Recipes.Managers.RecipeManager;

namespace Bureau.Recipes.Models
{
    internal class RecipeAggregate
    {
        public TermEntry Header { get; set; }
        public FlexibleRecord<RecipeDetails> Details { get; set; }
        public HashSet<Edge> Edges { get; set; } = new HashSet<Edge>();
        public HashSet<TermEntry> Ingredients { get; set; } = new HashSet<TermEntry>();
        public HashSet<FlexibleRecord<QuantityDetails>> IngredientsDetails { get; set; } = new HashSet<FlexibleRecord<QuantityDetails>>();

    }
}
