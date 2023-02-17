using System;
using System.Web;

namespace ModulesLibrary
{
    public class EventsModule : BaseModule
    {
        protected override void InvokeEvent(HttpContext context, string name)
        {
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
