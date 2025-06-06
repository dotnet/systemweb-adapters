using System.Web;
using System.Web.Hosting;

internal static class HostingEnvironmentEndpointExtensions
{
    public static void MapHostingEnvironmentDetails(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/hosting", () => new
        {
            HostingEnvironment = new
            {
                HostingEnvironment.IsHosted,
                HostingEnvironment.ApplicationID,
                HostingEnvironment.SiteName,
            },
            HttpRuntime = new
            {
                HttpRuntime.AppDomainAppVirtualPath,
                HttpRuntime.AppDomainAppPath,
            },
        });
    }
}
