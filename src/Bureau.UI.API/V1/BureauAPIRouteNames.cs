namespace Bureau.UI.API.V1
{
    public class BureauAPIRouteNames
    {
        #region notes

        public const string NotesGroup = "notes";

        public const string CreateTextNote = "create-text-note";
        public const string GetTextNoteById = "get-text-note-by-id";

        #endregion notes

        #region recipes

        public const string RecipesGroup = "recipes";
        public const string GetRecipes = "get-recipes";
        public const string GetRecipeById = "get-recipe-by-id";
        public const string CreateRecipes = "create-recipes";
        public const string UpdateRecipes = "update-recipes";
        public const string DeleteRecipes = "delete-recipes";
        #endregion recipes
    }
}
