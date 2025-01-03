using Bureau.Core;

namespace Bureau.Recipes.Factories
{
    public static class RecipeResultErrorFactory
    {
        public static ResultError GroupNotFound(string groupTermId) => new ResultError($"Group term with Id = {groupTermId} not present.");
        public static ResultError RecipeNotFound(string recipeTermId) => new ResultError($"Recipe term with Id = {recipeTermId} not present.");
        public static ResultError RecipeIdBadFormat(string recipeId) => new ResultError($"Recipe with Id = {recipeId} has a bad format for the requested operation.");
        public static ResultError IngredientNotFound(string ingredientTermId) => new ResultError($"Ingredient term with Id = {ingredientTermId} not present.");
        public static ResultError InstructionsNotFound(string instructionId) => new ResultError($"Instruction with Id = {instructionId} not present.");
        public static ResultError UnknownEdgeType(string edgeId, int edgeType) => new ResultError($"Unknown edge type ({edgeType}) in Recipe module with Id = {edgeId}.");
        public static ResultError UnknownEdgeReference(string edgeId, string refId) => new ResultError($"Unknown reference ({refId}) in edge with Id = {edgeId}.");
        public static ResultError UnknownTerm(string term) => new ResultError($"Unknown term ({term}).");
        public static ResultError ParentNodeNotFound(string edgeId) => new ResultError($"Parent node not defined but expected. On edge with Id = {edgeId}.");
    }
}
