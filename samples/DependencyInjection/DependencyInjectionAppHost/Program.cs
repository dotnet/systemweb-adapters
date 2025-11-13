var builder = DistributedApplication.CreateBuilder(args);

var frameworkApp = builder.AddIISExpressProject<Projects.DependencyInjectionFramework>("framework")
    .WithHttpHealthCheck("/health");

builder.Build().Run();
