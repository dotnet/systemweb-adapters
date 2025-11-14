// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

/// <summary>
/// A resource representing an IIS Express project.
/// </summary>
/// <param name="name">Name of the resource</param>
/// <param name="is64Bit">Whether IIS Express should run as 64-bit.</param>
/// <param name="projectPath">The path to the project.</param>
public class IISExpressProjectResource(string name, bool is64Bit, string projectPath)
    : ExecutableResource(name, Path.Combine(GetIISExpressDir(is64Bit), "iisexpress.exe"), Path.GetDirectoryName(projectPath)!)
{
    internal const string ApplicationHostFileName = "applicationHost.config";

    private static string GetIISExpressDir(bool is64Bit) =>
        Path.Combine(is64Bit ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) : Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "IIS Express");

    internal string DefaultConfigurationPath => Path.Combine(GetIISExpressDir(is64Bit), "config", "templates", "PersonalWebServer", ApplicationHostFileName);
}
