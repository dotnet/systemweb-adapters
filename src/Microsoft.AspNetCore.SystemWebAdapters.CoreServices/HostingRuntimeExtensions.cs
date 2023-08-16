using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal static class HostingRuntimeExtensions
{
    public static void AddHostingRuntime(this IServiceCollection services)
    {
        services.TryAddSingleton<HostingEnvironmentAccessor>();
        services.TryAddSingleton<VirtualPathUtilityImpl>();
        services.TryAddSingleton<IMapPathUtility, MapPathUtility>();
        services.AddHostedService<HostingEnvironmentService>();
        services.AddOptions<HostingEnvironmentOptions>()
            .Configure(options =>
            {
                options.IsHosted = true;

                if (NativeMethods.IsAspNetCoreModuleLoaded())
                {
                    var config = NativeMethods.HttpGetApplicationProperties();

                    options.AppDomainAppVirtualPath = config.pwzVirtualApplicationPath;
                    options.AppDomainAppPath = config.pwzFullApplicationPath;
                }
            });
    }

    private sealed class HostingEnvironmentService : IHostedService
    {
        private readonly HostingEnvironmentAccessor _accessor;

        public HostingEnvironmentService(HostingEnvironmentAccessor accessor)
        {
            _accessor = accessor;
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            HostingEnvironmentAccessor.Current = _accessor;
            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            HostingEnvironmentAccessor.Current = null;
            return Task.CompletedTask;
        }
    }
}
