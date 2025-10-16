var builder = DistributedApplication.CreateBuilder(args);

var frameworkApp = builder.AddIISExpress("iis")
    .AddSiteProject<Projects.WebFormsToBlazorFramework>("framework")
    .WithDefaultIISExpressEndpoints()
    .WithOtlpExporter()
    .WithHttpHealthCheck();

var coreApp = builder.AddProject<Projects.WebFormsToBlazorCore>("core")
    .WithHttpHealthCheck()
    .WaitFor(frameworkApp);

var incremental = builder.AddIncrementalMigrationFallback(coreApp, frameworkApp)
    .WithSession();

builder.Build().Run();
