var builder = DistributedApplication.CreateBuilder(args);

var frameworkApp = builder.AddIISExpressProject<Projects.WebFormsToBlazorFramework>("framework")
    .WithHttpHealthCheck();

var coreApp = builder.AddProject<Projects.WebFormsToBlazorCore>("core")
    .WithHttpHealthCheck()
    .WaitFor(frameworkApp)
    .WithIncrementalMigrationFallback(frameworkApp, options => options.RemoteSession = RemoteSession.Enabled);

builder.Build().Run();
