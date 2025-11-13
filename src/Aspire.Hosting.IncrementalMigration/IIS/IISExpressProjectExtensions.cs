// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Xml;
using System.Xml.Linq;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Aspire.Hosting;

/// <summary>
/// Extensions for adding IIS Express projects to a distributed application.
/// </summary>
public static partial class IISExpressProjectExtensions
{
    [LoggerMessage(LogLevel.Error, "Could not find system.applicationHost/sites in the default IIS Express configuration.")]
    private static partial void LogSitesNotFound(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Created config file at {ConfigPath}")]
    private static partial void LogConfigPath(ILogger logger, string configPath);

    /// <summary>
    /// Adds an IIS Express project to the distributed application.
    /// </summary>
    /// <typeparam name="TProject">A type that represents the project reference.</typeparam>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource. This name will be used for service discovery when referenced in a dependency.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{IISExpressProjectResource}"/>.</returns>
    public static IResourceBuilder<IISExpressProjectResource> AddIISExpressProject<TProject>(this IDistributedApplicationBuilder builder, string name)
        where TProject : IProjectMetadata, new()
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);

        if (!OperatingSystem.IsWindows() && builder.ExecutionContext.IsRunMode)
        {
            throw new InvalidOperationException("IIS projects can only be run on Windows.");
        }

        var project = new TProject();
        var launchProfile = project.GetLaunchDetails();

        return builder.AddResource<IISExpressProjectResource>(new(name, launchProfile.Use64BitIISExpress, project.ProjectPath))
            .WithAnnotation(project)
            .WithEndpoints(launchProfile)
            .WithDebugger()
            .WithIISConfigurationGenerator()
            .WithOtlpExporter(OtlpProtocol.HttpProtobuf);
    }

    /// <summary>
    /// Provides a way to configure the IIS Express project resource's site configuration.
    /// </summary>
    /// <param name="resource">The <see cref="IResourceBuilder{IISExpressProjectResource}"/>.</param>
    /// <param name="configure">A function to configure the site settings.</param>
    /// <returns>The <see cref="IResourceBuilder{IISExpressProjectResource}"/>.</returns>
    public static IResourceBuilder<IISExpressProjectResource> WithSiteConfiguration(this IResourceBuilder<IISExpressProjectResource> resource, Func<IISExpressProjectConfiguration, IISExpressProjectConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(configure);

        return resource.WithAnnotation(new IISExpressProjectConfigurationAnnotation(configure(resource.GetSiteConfiguration())), ResourceAnnotationMutationBehavior.Replace);
    }

    private static IISExpressProjectConfiguration GetSiteConfiguration(this IResourceBuilder<IISExpressProjectResource> resource)
    {
        if (resource.Resource.TryGetLastAnnotation<IISExpressProjectConfigurationAnnotation>(out var annotation))
        {
            return annotation.Configuration;
        }

        var config = new IISExpressProjectConfiguration() { SiteName = resource.Resource.Name };

        resource.WithAnnotation(new IISExpressProjectConfigurationAnnotation(config));

        return config;
    }

    private static IResourceBuilder<IISExpressProjectResource> WithDebugger(this IResourceBuilder<IISExpressProjectResource> builder)
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(5))
        {
            builder.WithVisualStudioDebuggingSupport();
        }

        return builder;
    }

    private static IResourceBuilder<IISExpressProjectResource> WithIISConfigurationGenerator(this IResourceBuilder<IISExpressProjectResource> builder)
    {
        builder.ApplicationBuilder.Eventing.Subscribe<BeforeResourceStartedEvent>(builder.Resource, (e, token) =>
        {
            var iis = builder.Resource;
            var logger = e.Services.GetRequiredService<ResourceLoggerService>().GetLogger(iis);
            var siteConfig = builder.GetSiteConfiguration();
            var site = CreateSite(iis, siteConfig, logger);

            var appHostConfig = iis.GetDefaultConfiguration();
            var configuration = appHostConfig.Descendants("configuration");

            var siteXml = configuration.Descendants("system.applicationHost")
                .Descendants("sites")
                .FirstOrDefault();

            if (siteXml is null)
            {
                LogSitesNotFound(logger);
                throw new InvalidOperationException("Invalid IIS Express configuration file.");
            }

            // Remove all existing sites
            siteXml.Descendants("site")
                .Remove();

            siteXml.Add(CreateSite(iis, siteConfig, logger));

            using var ms = SaveConfiguration(appHostConfig);

            var store = e.Services.GetRequiredService<IAspireStore>();
            var configPath = store.GetFileNameWithContent(IISExpressProjectResource.ApplicationHostFileName, ms);

            builder.WithArgs($"/apppool:{siteConfig.AppPool}");
            builder.WithArgs($"/config:{configPath}");
            builder.WithArgs($"/site:{siteConfig.SiteName}");

            LogConfigPath(logger, configPath);

            return Task.CompletedTask;
        });

        return builder;

        static XElement CreateSite(IISExpressProjectResource site, IISExpressProjectConfiguration siteConfig, ILogger logger)
            => new XElement("site",
                new XAttribute("name", siteConfig.SiteName),
                new XAttribute("id", siteConfig.SiteId),
                new XElement("application",
                    new XAttribute("path", siteConfig.ApplicationPath),
                    new XAttribute("applicationPool", siteConfig.AppPool),
                    new XElement("virtualDirectory",
                        new XAttribute("path", siteConfig.VirtualDirectoryPath),
                        new XAttribute("physicalPath", site.WorkingDirectory))),
                new XElement("bindings", CreateEndpointBindings(site)));

        static IEnumerable<XElement> CreateEndpointBindings(IResourceWithEndpoints project)
        {
            foreach (var endpoint in project.Annotations.OfType<EndpointAnnotation>())
            {
                var port = endpoint.TargetPort ?? endpoint.AllocatedEndpoint?.Port;
                if (port is { })
                {
                    yield return new XElement("binding",
                        new XAttribute("protocol", endpoint.UriScheme),
                        new XAttribute("bindingInformation", $"*:{port}:{endpoint.TargetHost}"));
                }
            }
        }

        static Stream SaveConfiguration(XDocument appHostConfig)
        {
            var ms = new MemoryStream();

            using (var writer = XmlWriter.Create(ms))
            {
                appHostConfig.Save(writer);
            }

            ms.Position = 0;

            return ms;
        }
    }

    private static XDocument GetDefaultConfiguration(this IISExpressProjectResource iis)
    {
        var configPath = iis.DefaultConfigurationPath;

        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException("Could not load default IIS Express configuration", configPath);
        }

        using var fs = File.OpenRead(configPath);
        using var reader = XmlReader.Create(fs, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });

        return XDocument.Load(reader);
    }
}
