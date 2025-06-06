using System.Web;
using MachineKeyExample;

namespace MachineKeyFramework
{
    public class MachineKeyExampleHandler : IHttpHandler
    {
        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            context.ProcessMachineKeyRequest();
        }
    }
}
