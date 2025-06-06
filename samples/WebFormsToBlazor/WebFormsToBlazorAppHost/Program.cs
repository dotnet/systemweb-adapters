var builder = DistributedApplication.CreateBuilder(args);

var remoteApiKey = builder.AddParameter("apiKey", Guid.NewGuid().ToString(), secret: true);

var frameworkApp = builder.AddIISExpress("iis")
    .AddSiteProject<Projects.WebFormsToBlazorFramework>("framework")
    .WithDefaultIISExpressEndpoints()
    .WithEnvironment("RemoteApp__ApiKey", remoteApiKey)
    .WithHttpHealthCheck();

var coreApp = builder.AddProject<Projects.WebFormsToBlazorCore>("core")
    .WithEnvironment("RemoteApp__ApiKey", remoteApiKey)
    .WithEnvironment("RemoteApp__Url", frameworkApp.GetEndpoint("https"))
    .WithHttpHealthCheck()
    .WaitFor(frameworkApp);

builder.Build().Run();
