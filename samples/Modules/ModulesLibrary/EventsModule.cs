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

            if (string.Equals(context.Request.QueryString["action"], "end", StringComparison.Ordinal) && string.Equals(context.Request.QueryString["notification"], context.CurrentNotification.ToString(), StringComparison.Ordinal))
            {
                context.Response.End();
            }
        }
    }
}
