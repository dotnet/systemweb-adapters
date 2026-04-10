var builder = DistributedApplication.CreateBuilder(args);

var frameworkApp = builder.AddIISExpressProject<Projects.WebFormsToBlazorFramework>("framework")
    .WithHttpHealthCheck();

builder.AddProject<Projects.BlazorWebApp>("core")
    .WaitFor(frameworkApp)
    .WithIncrementalMigrationFallback(frameworkApp, options => options.RemoteSession = RemoteSession.Enabled);

builder.Build().Run();
