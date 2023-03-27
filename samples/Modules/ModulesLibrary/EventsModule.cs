using System;
using System.Web;

namespace ModulesLibrary
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1510:Use ArgumentNullException throw helper", Justification = "Source shared with .NET Framework that does not have the method")]
    public class EventsModule : BaseModule
    {
        protected override void InvokeEvent(HttpContext context, string name)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Response.ContentType = "text/plain";

            context.Response.Output.WriteLine(name);

            if (string.Equals(name, context.Request.QueryString["notification"], StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(context.Request.QueryString["action"], "end", StringComparison.Ordinal))
                {
                    context.Response.End();
                }
                else if (string.Equals(context.Request.QueryString["action"], "complete", StringComparison.Ordinal))
                {
                    context.ApplicationInstance.CompleteRequest();
                }
            }
        }
    }
}
