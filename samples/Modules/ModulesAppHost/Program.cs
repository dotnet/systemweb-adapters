var builder = DistributedApplication.CreateBuilder(args);

var frameworkApp = builder.AddIISExpress("iis")
    .AddSiteProject<Projects.ModulesFramework>("framework")
    .WithDefaultIISExpressEndpoints()
    .WithOtlpExporter()
    .WithHttpHealthCheck();

var coreApp = builder.AddProject<Projects.ModulesCore>("core")
    .WithHttpHealthCheck();

builder.Build().Run();
