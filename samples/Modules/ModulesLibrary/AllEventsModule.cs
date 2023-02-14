using System.Web;

namespace ModulesLibrary
{
    public class AllEventsModule : BaseModule
    {
        protected override void InvokeEvent(HttpContext context, string name)
        {
            context.Response.Output.WriteLine(name);
        }
    }
}
