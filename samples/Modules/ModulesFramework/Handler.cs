using System.Web;

namespace ModulesFramework
{
    public class Handler : IHttpHandler
    {
        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.Write($"State: {context.CurrentNotification}\n");
            context.Response.Flush();
            context.Response.Write($"State: {context.CurrentNotification}\n");
            context.Response.Flush();
            context.Response.Write($"State: {context.CurrentNotification}\n");
        }
    }
}
