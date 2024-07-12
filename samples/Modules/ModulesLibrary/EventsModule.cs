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

        public override void Init(HttpApplication application)
        {
            if (application is { })
            {
                application.BeginRequest += (s, o) => ((HttpApplication)s!).Context.Response.ContentType = "text/plain";

                base.Init(application);
            }
        }

        protected override void InvokeEvent(HttpContext context, string name)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var action = context.Request.QueryString["action"];

            var writeOutputBefore = action != Throw;

            if (writeOutputBefore)
            {
                context.Response.Output.WriteLine(name);
            }

            if (string.Equals(name, context.Request.QueryString["notification"], StringComparison.OrdinalIgnoreCase))
            {
                switch (action)
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

            if (!writeOutputBefore)
            {
                context.Response.Output.WriteLine(name);
            }
        }
    }
}
