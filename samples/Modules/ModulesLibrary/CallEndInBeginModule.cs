using System.Web;

namespace ModulesLibrary
{
    public class CallEndInBeginModule : BaseModule
    {
        protected override void InvokeEvent(HttpContext context, string name)
        {
            context.Response.Output.WriteLine(name);

            if (context.CurrentNotification == RequestNotification.BeginRequest)
            {
                context.Response.End();
            }
        }
    }
}
