using System.Globalization;
using System.Text.RegularExpressions;

namespace BlazorSessionCore
{
    internal sealed class AxdContraint : IRouteConstraint
    {
        private readonly Regex regex;

        public AxdContraint()
        {
            regex = new Regex(@"^*.axd", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));
        }

        public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var isMatch = false;
            if (values.TryGetValue(routeKey, out object? value))
            {
                var parameterValueString = Convert.ToString(value, CultureInfo.InvariantCulture);
                if (parameterValueString != null)
                {
                    isMatch = regex.IsMatch(parameterValueString);
                }
            }

            return isMatch;
        }
    }
}
