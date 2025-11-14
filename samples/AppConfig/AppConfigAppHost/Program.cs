var builder = DistributedApplication.CreateBuilder(args);

var frameworkApp = builder.AddIISExpressProject<Projects.AppConfigFramework>("framework")
    .WithHttpHealthCheck("/");

var core = builder.AddProject<Projects.AppConfigCore>("core")
    .WithHttpHealthCheck("/");

builder.Build().Run();
