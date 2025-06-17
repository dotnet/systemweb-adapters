using Microsoft.AspNetCore.SystemWebAdapters.Aspire;
using System.Security.Policy;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddSqlServer("identityserver")
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("identity");

var frameworkApp = builder.AddIISExpress("iis")
    .AddSiteProject<Projects.AuthRemoteIdentityFramework>("framework")
    .WithDefaultIISExpressEndpoints()
    .WithReference(db, connectionName: "DefaultConnection")
    .WithOtlpExporter()
    .WaitFor(db)
    .WithHttpHealthCheck();

var coreApp = builder.AddProject<Projects.AuthRemoteIdentityCore>("core")
    .WithHttpHealthCheck()
    .WaitFor(frameworkApp)
    .WithIncrementalMigrationFallback(frameworkApp, remoteAuthentication: RemoteAuthentication.DefaultScheme);

builder.Build().Run();
