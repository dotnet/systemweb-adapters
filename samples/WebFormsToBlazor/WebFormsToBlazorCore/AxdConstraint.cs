using System.Globalization;
using System.Text.RegularExpressions;

namespace WebFormsToBlazorCore;

internal sealed class AxdConstraint : IRouteConstraint
{
    private readonly Regex regex;

    public AxdConstraint()
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
