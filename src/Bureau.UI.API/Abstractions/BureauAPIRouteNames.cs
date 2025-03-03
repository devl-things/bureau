﻿namespace Bureau.UI.API
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

        #region calendar

        public const string CalendarsGroup = "calendars";
        public const string GetCalendars = "get-calendars";
        public const string GetCalendarById = "get-calendar-by-id";
        public const string CreateCalendar = "create-calendar";
        public const string UpdateCalendar = "update-calendar";
        public const string DeleteCalendar = "delete-calendar";

        #endregion calendar
    }
}
