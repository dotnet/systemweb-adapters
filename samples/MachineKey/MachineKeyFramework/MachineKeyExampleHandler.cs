using System.Web;

namespace MachineKeyFramework
{
    public class MachineKeyExampleHandler : IHttpHandler
    {
        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            MachineKeyExample.MachineKeyTest.Run(context);
        }
    }
}
