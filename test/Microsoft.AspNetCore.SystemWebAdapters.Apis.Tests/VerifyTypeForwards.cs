// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.SystemWebAdapters;

/// <summary>
/// This test validates that the *Base type (the abstraction in System.Web) has the same members as on the non-base type. For example, <see cref="HttpContext"/> and <see cref="HttpContextBase"/>.
/// This test is a best effort to ensure when an API is added to the main type, the base type gets it as well
/// </summary>
public class VerifyHttpBaseTypes
{
    private static readonly HashSet<string> _skipped = new HashSet<string>()
    {
        "op_Implicit", // This is only defined on the non-base types as there is a well defined conversion
    };

    private readonly ITestOutputHelper _output;

    public VerifyHttpBaseTypes(ITestOutputHelper output)
    {
        _output = output;
    }

    [InlineData(typeof(HttpContext), typeof(HttpContextBase))]
    [InlineData(typeof(HttpRequest), typeof(HttpRequestBase))]
    [InlineData(typeof(HttpResponse), typeof(HttpResponseBase))]
    [Theory]
    public void ValidateMemberExistsOnBaseType(Type type, Type baseType)
    {
        const BindingFlags Flags = BindingFlags.Public | BindingFlags.Instance;

        var isMissing = false;

        foreach (var method in type.GetMethods(Flags))
        {
            if (_skipped.Contains(method.Name))
            {
                continue;
            }

            var found = baseType.GetMethod(method.Name, Flags, method.GetParameters().Select(p => p.ParameterType).ToArray());

            if (found is null)
            {
                _output.WriteLine(method.Name);
                isMissing = true;
            }
        }

        Assert.False(isMissing);
    }
}

public class VerifyTypeForwards
{
    private readonly ITestOutputHelper _output;

    public VerifyTypeForwards(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void VerifyPublicTypesAreForwardedToSystemWeb()
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "adapters", "netfx");
        using var frameworkContext = new MetadataLoadContext(new PathAssemblyResolver(Directory.EnumerateFiles(dir, "*.dll")));

        var systemWeb = frameworkContext.LoadFromAssemblyName("System.Web");

        var exportedTypes = typeof(HttpContextBase).Assembly
            .GetExportedTypes()
            .Select(t => t.FullName!)
            .Where(t => systemWeb.GetType(t) is not null)
            .ToHashSet();

        WriteExpectedFile(exportedTypes);

        var forwardedTypes = frameworkContext.LoadFromAssemblyName("Microsoft.AspNetCore.SystemWebAdapters")
            .GetForwardedTypes()
            .Select(t => t.FullName!);

        // All the exported types (and only them) should be forwarded
        Assert.True(exportedTypes.SetEquals(forwardedTypes));
    }

    private void WriteExpectedFile(IEnumerable<string> types)
    {
        _output.WriteLine("// Ensure the following type forwards are present in the .NET Framework compilation of the adapters");
        _output.WriteLine(string.Empty);
        _output.WriteLine("using System.Runtime.CompilerServices;");
        _output.WriteLine(string.Empty);

        foreach (var type in types.OrderBy(t => t))
        {
            _output.WriteLine($"[assembly: TypeForwardedTo(typeof(global::{type}))]");
        }
    }
}
