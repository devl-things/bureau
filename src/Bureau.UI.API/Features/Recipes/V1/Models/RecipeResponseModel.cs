namespace Bureau.UI.API.Features.Recipes.V1.Models
{
    public class RecipeResponseModel : RecipeRequestModel
    {
        public required string Id { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
