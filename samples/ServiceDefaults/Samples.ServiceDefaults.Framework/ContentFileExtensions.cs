// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web;
using System.Web.Hosting;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace System.Web;

public static class ContentFileExtensions
{
    /// <summary>
    /// When using the <code>CopyContentFiles="true"</code> with PackageReference the 'Content' files will be copied to the bin directory, but does not get picked up when launching from VS.
    /// This adds a <see cref="VirtualPathProvider"/> that helps resolve those files as well as a module that will serve them up during development.
    /// </summary>
    /// <remarks>For the samples, there is a custom target that will add a file called 'contentDirectories.txt' with each directory that had a 'Content' folder.</remarks>
    /// <param name="services"></param>
    public static ISystemWebAdapterBuilder AddVirtualizedContentDirectories(this ISystemWebAdapterBuilder builder)
    {
        if (GetProvider() is { } provider)
        {
            HostingEnvironment.RegisterVirtualPathProvider(new FileProviderVirtualPathProvider(provider));

            builder.Services.AddSingleton<IHttpModule>(new StaticFileProviderHttpModule(provider));
        }

        return builder;
    }

    private static CompositeFileProvider? GetProvider()
    {
        var binDir = Path.Combine(HttpRuntime.BinDirectory, "contentDirectories.txt");

        if (File.Exists(binDir))
        {
            var providers = new List<IFileProvider>();

            foreach (var line in File.ReadAllLines(binDir))
            {
                if (Directory.Exists(line))
                {
                    providers.Add(new PhysicalFileProvider(line));
                }
            }

            if (providers.Count != 0)
            {
                return new CompositeFileProvider(providers);
            }
        }

        return null;
    }
}
