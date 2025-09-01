var builder = DistributedApplication.CreateBuilder(args);

var frameworkApp = builder.AddIISExpress("iis")
    .AddSiteProject<Projects.AuthRemoteFormsAuthFramework>("framework")
    .WithDefaultIISExpressEndpoints()
    .WithOtlpExporter()
    .WithHttpHealthCheck();

var coreApp = builder.AddProject<Projects.AuthRemoteFormsAuthCore>("core")
    .WithHttpHealthCheck()
    .WaitFor(frameworkApp);

var incremental = builder.AddIncrementalMigrationFallback(coreApp, frameworkApp)
    .WithAuthentication(RemoteAuthentication.DefaultScheme);

builder.Build().Run();
