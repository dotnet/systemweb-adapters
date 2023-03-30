using System;
using System.Web;

namespace ModulesLibrary
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1510:Use ArgumentNullException throw helper", Justification = "Source shared with .NET Framework that does not have the method")]
    public class EventsModule : BaseModule
    {
        public const string End = "end";
        public const string Complete = "complete";
        public const string Throw = "throw";

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
                switch (context.Request.QueryString["action"])
                {
                    case End:
                        context.Response.End();
                        break;
                    case Complete:
                        context.ApplicationInstance.CompleteRequest();
                        break;
                    case Throw:
                        throw new InvalidOperationException();
                }
            }
        }
    }
}
