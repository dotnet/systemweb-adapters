var builder = DistributedApplication.CreateBuilder(args);

var frameworkApp = builder.AddIISExpressProject<Projects.MachineKeyFramework>("framework")
    .WithHttpHealthCheck();

var coreApp = builder.AddProject<Projects.MachineKeyCore>("core")
    .WithHttpHealthCheck();

builder.Build().Run();
