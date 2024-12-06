using C3D.Extensions.Aspire.IISExpress;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddIISExpressConfiguration(ThisAssembly.Project.SolutionName, ThisAssembly.Project.SolutionDir);

var remoteSessionFramework = builder.AddIISExpressProject<Projects.RemoteSessionFramework>("RemoteSessionFramework");
var remoteSession = builder.AddProject<Projects.RemoteSessionCore>("RemoteSession")
    .WithReference(remoteSessionFramework);

builder.Build().Run();
