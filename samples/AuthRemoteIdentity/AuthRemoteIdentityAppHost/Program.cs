using System.Security.Policy;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("sqlPass", secret: true);
var apiKey = builder.AddParameter("remoteapp-apiKey", () => Guid.NewGuid().ToString(), secret: true);

var db = builder.AddSqlServer("identityserver", password: password)
    // Configure the container to store data in a volume so that it persists across instances.
    .WithDataVolume()
    // Keep the container running between app host sessions.
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("identity");

var frameworkApp = builder.AddIISExpressProject<Projects.AuthRemoteIdentityFramework>("framework")
    .WithReference(db, connectionName: "DefaultConnection")
    .WaitFor(db)
    .WithHttpHealthCheck();

var owin = builder.AddProject<Projects.AuthRemoteIdentityCore>("owin")
    .WithHttpEndpoint(targetPort: 5000)
    .WithHttpsEndpoint(targetPort: 5001)
    .WithHttpHealthCheck("/health")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("SAMPLE_MODE", "OWIN")
    .WithReference(db, connectionName: "DefaultConnection")
    .WithIncrementalMigrationFallback(frameworkApp, apiKey: apiKey)
    .WaitFor(frameworkApp)
    .WaitFor(db);

var coreApp = builder.AddProject<Projects.AuthRemoteIdentityCore>("core")
    .WithHttpEndpoint(targetPort: 5002)
    .WithHttpsEndpoint(targetPort: 5003)
    .WithHttpHealthCheck("/health")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("SAMPLE_MODE", "REMOTE")
    .WaitFor(frameworkApp)
    .WithIncrementalMigrationFallback(frameworkApp, options => options.RemoteAuthentication = RemoteAuthentication.DefaultScheme, apiKey);

builder.Build().Run();
