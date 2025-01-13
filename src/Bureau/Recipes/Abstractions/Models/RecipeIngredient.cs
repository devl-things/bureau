using Bureau.Models;

namespace Bureau.Recipes.Models
{
    public class RecipeIngredient
    {
        public string Ingredient { get; set; }
        public QuantityDetails Quantity { get; set; }

        public RecipeIngredient(string ingredient)
        {
            Ingredient = ingredient;
        }

        public bool HasQuantity()
        {
            return !Quantity.IsEmpty();
        }
    }
}
