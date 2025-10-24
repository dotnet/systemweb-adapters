using System;
using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.SystemWebAdapters.Analyzers;

internal static class CompilationExtensions
{
    public static bool IsInAssembly(this ITypeSymbol type, string assemblyName)
    {
        return string.Equals(assemblyName, type.ContainingAssembly?.Name, StringComparison.Ordinal);
    }

    public static bool IsType(this Compilation compilation, ITypeSymbol type, string name)
    {
        return compilation.GetTypeByMetadataName(name) is { } targetType
            && SymbolEqualityComparer.Default.Equals(type, targetType);
    }
}
