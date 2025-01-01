using Bureau.Core.Models;
using Bureau.Models;

namespace Bureau.Recipes.Models
{
    [Obsolete("This class is obsolete. Use RecipeDto instead.")]
    internal class RecipeAggregate
    {
        public IReference RecipeReference { get; set; }
        public HashSet<TermEntry> TermEntries { get; set; } = new HashSet<TermEntry>(0);
        public FlexibleRecord<RecipeDetails> Details { get; set; } = new FlexibleRecord<RecipeDetails>(string.Empty);
        public HashSet<FlexibleRecord<QuantityDetails>> IngredientsDetails { get; set; } = new HashSet<FlexibleRecord<QuantityDetails>>(0);
        public HashSet<FlexibleRecord<NoteDetails>> Instructions { get; set; } = new HashSet<FlexibleRecord<NoteDetails>>(0);
        public HashSet<Edge> Edges { get; set; } = new HashSet<Edge>(0);

        public RecipeAggregate(IReference recipeReference)
        {
            RecipeReference = recipeReference;
        }
    }
}
