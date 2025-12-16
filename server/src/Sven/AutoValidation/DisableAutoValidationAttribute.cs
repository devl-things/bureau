using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Sven.AutoValidation
{

    [AttributeUsage(AttributeTargets.Method)]
    public class DisableAutoValidationAttribute : Attribute, IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            // Remove the built-in ModelStateInvalidFilterFactory
            var filterToRemove = action.Filters
                .FirstOrDefault(f => f.GetType().Name == "ModelStateInvalidFilterFactory");

            if (filterToRemove != null)
            {
                action.Filters.Remove(filterToRemove);
            }
        }
    }
}
