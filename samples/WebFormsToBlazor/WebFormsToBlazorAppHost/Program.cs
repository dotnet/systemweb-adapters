using Microsoft.AspNetCore.SystemWebAdapters.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var frameworkApp = builder.AddIISExpress("iis")
    .AddSiteProject<Projects.WebFormsToBlazorFramework>("framework")
    .WithDefaultIISExpressEndpoints()
    .WithOtlpExporter()
    .WithHttpHealthCheck();

var coreApp = builder.AddProject<Projects.WebFormsToBlazorCore>("core")
    .WithHttpHealthCheck()
    .WaitFor(frameworkApp)
    .WithIncrementalMigrationFallback(frameworkApp, options => options.RemoteSession = RemoteSession.Enabled);

builder.Build().Run();
