// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.SystemWebAdapters.Analyzers;

internal static class SymbolExtensions
{
    public static ISymbol? GetMember(this INamedTypeSymbol type, string name, params ISymbol[] parameters)
    {
        foreach (var member in type.GetMembers(name))
        {
            if (member is IPropertySymbol property && property.Parameters.Length == parameters.Length)
            {
                var match = true;

                for (int i = 0; i < parameters.Length; i++)
                {
                    match &= SymbolEqualityComparer.Default.Equals(parameters[i], property.Parameters[i].Type);
                }

                if (match)
                {
                    return property;
                }
            }
            else if (member is IMethodSymbol method && method.Parameters.Length == parameters.Length)
            {
                var match = true;

                for (int i = 0; i < parameters.Length; i++)
                {
                    match &= SymbolEqualityComparer.Default.Equals(parameters[i], method.Parameters[i].Type);
                }

                if (match)
                {
                    return method;
                }
            }
        }

        if (type.BaseType is { } baseType)
        {
            return baseType.GetMember(name, parameters);
        }

        return null;
    }

}
