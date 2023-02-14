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

            if (string.Equals(context.Request.QueryString["action"], "end", StringComparison.Ordinal))
            {
                var isPost = false;
                var notification = context.Request.QueryString["notification"];

                if (notification.StartsWith("Post", StringComparison.OrdinalIgnoreCase))
                {
                    notification = notification.Substring(4);
                    isPost = true;
                }

                if (string.Equals(notification, context.CurrentNotification.ToString(), StringComparison.OrdinalIgnoreCase) && isPost == context.IsPostNotification)
                {
                    context.Response.End();
                }
            }
        }
    }
}
