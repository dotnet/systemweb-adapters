var builder = DistributedApplication.CreateBuilder(args);

var frameworkApp = builder.AddIISExpressProject<Projects.ModulesFramework>("framework")
    .WithHttpHealthCheck();

var coreApp = builder.AddProject<Projects.ModulesCore>("core")
    .WithHttpHealthCheck();

builder.Build().Run();
