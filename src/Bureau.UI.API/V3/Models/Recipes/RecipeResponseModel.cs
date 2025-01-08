using Bureau.UI.API.Models;

namespace Bureau.UI.API.V3.Models.Recipes
{
    public class RecipeResponseModel : RecipeRequestModel, IResponseId
    {
        public required string Id { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
