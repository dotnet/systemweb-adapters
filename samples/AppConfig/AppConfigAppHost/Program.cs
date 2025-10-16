var builder = DistributedApplication.CreateBuilder(args);

var frameworkApp = builder.AddIISExpress("iis")
    .AddSiteProject<Projects.AppConfigFramework>("framework")
    .WithDefaultIISExpressEndpoints()
    .WithOtlpExporter()
    .WithHttpHealthCheck("/");

var core = builder.AddProject<Projects.AppConfigCore>("core")
    .WithHttpHealthCheck("/");

builder.Build().Run();
