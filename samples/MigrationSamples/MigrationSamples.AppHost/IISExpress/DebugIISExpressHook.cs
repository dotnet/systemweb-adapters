using Aspire.Hosting.ApplicationModel;
using C3D.Extensions.Aspire.IISExpress.Resources;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace C3D.Extensions.Aspire.IISExpress;

internal class DebugIISExpressHook : BackgroundService
{
    private readonly ILogger logger;
    private readonly ResourceNotificationService resourceNotificationService;

    public DebugIISExpressHook(
        ILogger<DebugIISExpressHook> logger, 
        ResourceNotificationService resourceNotificationService
        )
    {
        this.logger = logger;
        this.resourceNotificationService = resourceNotificationService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var notification in resourceNotificationService.WatchAsync(stoppingToken))
        {
            if (notification.Resource is IISExpressProjectResource resource && 
                resource.Annotations.OfType<DebugAttachResource>().Any(dar=>dar.DebugMode==DebugMode.VSJITDebugger) &&
                !resource.Annotations.OfType<DebugerAttachedResource>().Any())
            {
                var processId = notification.Snapshot.Properties.SingleOrDefault(prp => prp.Name == "executable.pid")?.Value as int? ?? 0;
                if (processId !=0)
                {
                    var processInfo = new ProcessStartInfo("vsjitdebugger.exe", $"-p {processId}")
                    {
                        CreateNoWindow = true
                    };
                    logger.LogInformation("Attaching Debugger to IIS Express {processId} for {applicationName}", processId, notification.Resource.Name);
                    Process? process = Process.Start(processInfo);
                    if (process is not null)
                    {
                        logger.LogInformation("Attached Debugger using vsjitdebugger.exe -p {processId}", processId);
                        resource.Annotations.Add(new DebugerAttachedResource());
                    }
                }
            }
        }
    }
}