using Microsoft.AspNetCore.SystemWebAdapters.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var frameworkApp = builder.AddIISExpress("iis")
    .AddSiteProject<Projects.AuthRemoteFormsAuthFramework>("framework")
    .WithDefaultIISExpressEndpoints()
    .WithOtlpExporter()
    .WithHttpHealthCheck();

var coreApp = builder.AddProject<Projects.AuthRemoteFormsAuthCore>("core")
    .WithHttpHealthCheck()
    .WaitFor(frameworkApp)
    .WithIncrementalMigrationFallback(frameworkApp, options => options.RemoteAuthentication = RemoteAuthentication.DefaultScheme);

builder.Build().Run();
