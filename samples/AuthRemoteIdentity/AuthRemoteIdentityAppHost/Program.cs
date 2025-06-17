using System.Security.Policy;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var remoteApiKey = builder.AddParameter("apiKey", Guid.NewGuid().ToString(), secret: true);

var iisExpress = builder.AddIISExpress("iis");

var db = builder.AddSqlServer("identityserver")
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("identity");

var frameworkApp = iisExpress.AddSiteProject<Projects.AuthRemoteIdentityFramework>("framework")
    .WithDefaultIISExpressEndpoints()
    .WithEnvironment("RemoteApp__ApiKey", remoteApiKey)
    .WithReference(db, connectionName: "DefaultConnection")
    .WithOtlpExporter()
    .WaitFor(db)
    .WithHttpHealthCheck();

var coreApp = builder.AddProject<Projects.AuthRemoteIdentityCore>("core")
    .WithEnvironment("RemoteApp__ApiKey", remoteApiKey)
    .WithEnvironment("RemoteApp__Url", frameworkApp.GetEndpoint("https"))
    .WithHttpHealthCheck()
    .WaitFor(frameworkApp);

builder.Build().Run();
