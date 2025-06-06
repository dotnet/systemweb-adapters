var builder = DistributedApplication.CreateBuilder(args);

var remoteApiKey = builder.AddParameter("apiKey", Guid.NewGuid().ToString(), secret: true);

var frameworkApp = builder.AddIISExpress("iis")
    .AddSiteProject<Projects.SessionRemoteFramework>("framework")
    .WithDefaultIISExpressEndpoints()
    .WithEnvironment("RemoteApp__ApiKey", remoteApiKey)
    .WithHttpHealthCheck(path: "/framework");

var coreApp = builder.AddProject<Projects.SessionRemoteCore>("core")
    .WithEnvironment("RemoteApp__ApiKey", remoteApiKey)
    .WithEnvironment("RemoteApp__Url", frameworkApp.GetEndpoint("https"))
    .WithHttpHealthCheck()
    .WaitFor(frameworkApp);

builder.Build().Run();
