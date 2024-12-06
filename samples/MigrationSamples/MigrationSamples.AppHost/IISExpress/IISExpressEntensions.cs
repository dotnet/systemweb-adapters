using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using C3D.Extensions.Aspire.IISExpress.Configuration;
using C3D.Extensions.Aspire.IISExpress.Resources;
using System.Collections.Immutable;
using System.Xml.Serialization;

namespace C3D.Extensions.Aspire.IISExpress;

public static class IISExpressEntensions
{
    public static IResourceBuilder<IISExpressProjectResource> WithDebugger(this IResourceBuilder<IISExpressProjectResource> resourceBuilder,
        DebugMode debugMode = DebugMode.VSJITDebugger) =>
        resourceBuilder
            .WithEnvironment("Launch_Debugger_On_Start", debugMode == DebugMode.Environment ? "true" : null)
            .WithAnnotation<DebugAttachResource>(new() { DebugMode = debugMode }, ResourceAnnotationMutationBehavior.Replace);

    /// <summary>
    /// Adds the configuration information used to find the applicationhost.config file
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="solutionName">This can be set from ThisAssembly.Project.SolutionName</param>
    /// <param name="solutionDir">This can be set from ThisAssembly.Project.SolutionDir</param>
    /// <returns></returns>
    public static IDistributedApplicationBuilder AddIISExpressConfiguration(this IDistributedApplicationBuilder builder,
        string solutionName,
        string? solutionDir = null)
    {
        solutionDir ??= new DirectoryInfo(builder.AppHostDirectory).Parent!.FullName;

        builder.AddResource(new IISExpressConfigurationResource(solutionName, solutionDir))
            .WithInitialState(new CustomResourceSnapshot()
            {
                ResourceType = "IISExpress",
                Properties = ImmutableArray<ResourcePropertySnapshot>.Empty,
                State = KnownResourceStates.Hidden
            })
            .ExcludeFromManifest();

        return builder;
    }

    public static IResourceBuilder<IISExpressProjectResource> AddIISExpressProject<T>(this IDistributedApplicationBuilder builder,
        string? resourceName = null,
        IISExpressBitness? bitness = null)
        where T : IProjectMetadata, new()
    {
        var app = new T();

        var appName = app.GetType().Name;
        var projectPath = System.IO.Path.GetDirectoryName(app.ProjectPath)!;
        var programFiles = System.Environment.GetFolderPath(bitness == IISExpressBitness.IISExpress32Bit ?
            Environment.SpecialFolder.ProgramFilesX86 :
            Environment.SpecialFolder.ProgramFiles);
        var iisExpress = System.IO.Path.Combine(programFiles, "IIS Express", "iisexpress.exe");
        var config = builder.Resources.OfType<IISExpressConfigurationResource>().SingleOrDefault()
            ?? throw new Exception("AddIISExpressConfiguration has not been called");

        var applicationHostConfigPath = System.IO.Path.Combine(
            config.SolutionDir,
            ".vs",
            config.SolutionName,
            "config",
            "applicationhost.config");

        var resource = new IISExpressProjectResource(resourceName ?? appName, iisExpress, projectPath);

        var resourceBuilder = builder.AddResource(resource)
            .WithAnnotation(app)
            .WithAnnotation(new IISExpressSiteAnnotation(applicationHostConfigPath, appName))
            .WithArgs(
                $"/config:{applicationHostConfigPath}",
                $"/site:{appName}",
                $"/apppool:Clr4IntegratedAppPool")
            .WithOtlpExporter()
            .ExcludeFromManifest();

        var siteConfig = GetSiteConfig(applicationHostConfigPath, appName);

        if (siteConfig is not null)
        {
            foreach (var binding in siteConfig.Bindings)
            {
                resourceBuilder.WithEndpoint(binding.Protocol, e =>
                {
                    e.Port = binding.Port;
                    e.UriScheme = binding.Protocol;
                    e.IsProxied = false;
                },
                createIfNotExists: true);
            }
        }

        return resourceBuilder;
    }

    private static Site? GetSiteConfig(string appHostConfigPath, string siteName)
    {
        var serializer = new XmlSerializer(typeof(ApplicationHostConfiguration));
        using var reader = new FileStream(appHostConfigPath, FileMode.Open);

        if (serializer.Deserialize(reader) is not ApplicationHostConfiguration appHostConfig)
        {
            return null;
        }

        return appHostConfig.SystemApplicationHost.Sites
            .SingleOrDefault(s => string.Equals(s.Name, siteName, StringComparison.OrdinalIgnoreCase));
    }
}
