var builder = DistributedApplication.CreateBuilder(args);

var frameworkApp = builder.AddIISExpress("iis")
    .AddSiteProject<Projects.SessionRemoteFramework>("framework")
    .WithDefaultIISExpressEndpoints()
    .WithOtlpExporter()
    .WithHttpHealthCheck(path: "/framework");

var coreApp = builder.AddProject<Projects.SessionRemoteCore>("core")
    .WithHttpHealthCheck()
    .WaitFor(frameworkApp);

var incremental = builder.AddIncrementalMigrationFallback(coreApp, frameworkApp)
    .WithSession();

builder.Build().Run();
