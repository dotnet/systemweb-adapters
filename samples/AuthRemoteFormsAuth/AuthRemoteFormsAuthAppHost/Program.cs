var builder = DistributedApplication.CreateBuilder(args);

var remoteApiKey = builder.AddParameter("apiKey", Guid.NewGuid().ToString(), secret: true);

var frameworkApp = builder.AddIISExpress("iis")
    .AddSiteProject<Projects.AuthRemoteFormsAuthFramework>("framework")
    .WithDefaultIISExpressEndpoints()
    .WithOtlpExporter()
    .WithEnvironment("RemoteApp__ApiKey", remoteApiKey)
    .WithHttpHealthCheck();

var coreApp = builder.AddProject<Projects.AuthRemoteFormsAuthCore>("core")
    .WithEnvironment("RemoteApp__ApiKey", remoteApiKey)
    .WithReference(frameworkApp)
    .WithEnvironment("RemoteApp__Url", frameworkApp.GetEndpoint("https"))
    .WithHttpHealthCheck()
    .WaitFor(frameworkApp);

builder.Build().Run();
