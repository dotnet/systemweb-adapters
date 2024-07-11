// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.IO;
using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class VerifyMembersHaveSameSignature
{
    private readonly ITestOutputHelper _output;

    public VerifyMembersHaveSameSignature(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void VerifyMembersOnTypesAreConsistent()
    {
        var netstandard = GetDocumentationIds("adapters/netstandard/Microsoft.AspNetCore.SystemWebAdapters.dll", NetStandard20.References.All);
        var framework = GetDocumentationIds("adapters/netfx/Microsoft.AspNetCore.SystemWebAdapters.dll", Net472.References.All);
        var ok = File.ReadAllLines("BaselineOk.txt");

        netstandard.ExceptWith(framework);
        netstandard.ExceptWith(ok);

        foreach (var adaptedType in netstandard)
        {
            _output.WriteLine(adaptedType);
        }

        Assert.Empty(netstandard);
    }

    public static HashSet<string> GetDocumentationIds(string path, IEnumerable<PortableExecutableReference> referenceAssemblies)
    {
        var _adapter = MetadataReference.CreateFromFile(path);
        var _compilation = CSharpCompilation.Create(null)
            .AddReferences(_adapter)
            .AddReferences(referenceAssemblies);
        var Adapter = (IAssemblySymbol)_compilation.GetAssemblyOrModuleSymbol(_adapter)!;

        return TypeCollector.Collect(Adapter);
    }
}
