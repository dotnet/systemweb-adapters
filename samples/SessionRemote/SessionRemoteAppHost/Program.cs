var builder = DistributedApplication.CreateBuilder(args);

var frameworkApp = builder.AddIISExpressProject<Projects.SessionRemoteFramework>("framework")
    .WithHttpHealthCheck(path: "/framework");

var coreApp = builder.AddProject<Projects.SessionRemoteCore>("core")
    .WithHttpHealthCheck()
    .WaitFor(frameworkApp)
    .WithIncrementalMigrationFallback(frameworkApp, options => options.RemoteSession = RemoteSession.Enabled);

builder.Build().Run();
