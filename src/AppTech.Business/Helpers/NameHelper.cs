using System.Text.RegularExpressions;

namespace AppTech.Business.Helpers
{
    public static class NameHelper
    {
        public static string PluralizeControllerName(string controllerName)
        {
            if (string.IsNullOrWhiteSpace(controllerName))
                throw new ArgumentException("Controller name cannot be null or empty.", nameof(controllerName));

            var changedControllerName = string.Empty;

            if (controllerName.EndsWith("y"))
            {
                changedControllerName = Regex.Replace(controllerName, "y$", "ies");
            }
            else if (controllerName.EndsWith("s"))
            {
                changedControllerName = controllerName + "es";
            }
            else
            {
                changedControllerName = controllerName + "s";
            }

            return changedControllerName.ToLower();
        }
    }
}
