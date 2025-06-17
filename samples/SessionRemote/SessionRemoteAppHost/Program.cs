using Microsoft.AspNetCore.SystemWebAdapters.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var frameworkApp = builder.AddIISExpress("iis")
    .AddSiteProject<Projects.SessionRemoteFramework>("framework")
    .WithDefaultIISExpressEndpoints()
    .WithOtlpExporter()
    .WithHttpHealthCheck(path: "/framework");

var coreApp = builder.AddProject<Projects.SessionRemoteCore>("core")
    .WithHttpHealthCheck()
    .WaitFor(frameworkApp)
    .WithIncrementalMigrationFallback(frameworkApp, remoteSession: RemoteSession.Enabled);

builder.Build().Run();
