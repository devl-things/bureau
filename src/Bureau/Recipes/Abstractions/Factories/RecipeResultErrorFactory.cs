using Bureau.Core;

namespace Bureau.Recipes.Factories
{
    public static class RecipeResultErrorFactory
    {
        public static ResultError RecipeGroupNotFound(string groupTermId) => new ResultError($"Group term with Id = {groupTermId} not present.");
        public static ResultError RecipeNotFound(string recipeTermId) => new ResultError($"Recipe term with Id = {recipeTermId} not present.");
        public static ResultError RecipeIdBadFormat(string recipeId) => new ResultError($"Recipe with Id = {recipeId} has a bad format for the requested operation.");
        public static ResultError IngredientNotFound(string ingredientTermId) => new ResultError($"Ingredient term with Id = {ingredientTermId} not present.");
        public static ResultError InstructionsNotFound(string instructionId) => new ResultError($"Instruction with Id = {instructionId} not present.");
        public static ResultError UnknownRecipeEdgeType(string edgeId, int edgeType) => new ResultError($"Unknown edge type ({edgeType}) in Recipe module with Id = {edgeId}.");
    }
}
