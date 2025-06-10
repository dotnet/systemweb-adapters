var builder = DistributedApplication.CreateBuilder(args);

var frameworkApp = builder.AddIISExpress("iis")
    .AddSiteProject<Projects.MachineKeyFramework>("framework")
    .WithDefaultIISExpressEndpoints()
    .WithOtlpExporter()
    .WithHttpHealthCheck();

var coreApp = builder.AddProject<Projects.MachineKeyCore>("core")
    .WithHttpHealthCheck();

builder.Build().Run();
