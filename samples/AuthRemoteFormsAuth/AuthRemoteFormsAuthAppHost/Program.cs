var builder = DistributedApplication.CreateBuilder(args);

var frameworkApp = builder.AddIISExpressProject<Projects.AuthRemoteFormsAuthFramework>("framework")
    .WithHttpHealthCheck();

var coreApp = builder.AddProject<Projects.AuthRemoteFormsAuthCore>("core")
    .WithHttpHealthCheck()
    .WaitFor(frameworkApp)
    .WithIncrementalMigrationFallback(frameworkApp, options => options.RemoteAuthentication = RemoteAuthentication.DefaultScheme);

builder.Build().Run();
