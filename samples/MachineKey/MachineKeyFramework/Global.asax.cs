using System.IO;
using System.Web;
using MachineKeyExample;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.SystemWebAdapters.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MachineKeyFramework
{
    public class MachineKeyApplication : HttpApplication
    {
        protected void Application_Start()
        {
            HttpApplicationHost.RegisterHost(builder =>
            {
                builder.AddServiceDefaults();
                builder.AddDataProtection()
                    .SetApplicationName(MachineKeyExtensions.AppName)
                    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Path.GetTempPath(), "sharedkeys", MachineKeyExtensions.AppName)));
            });
        }
    }
}
