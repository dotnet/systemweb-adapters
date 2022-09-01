using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.SystemWebAdapters.Analyzers;

internal static class SymbolExtensions
{
    public static ISymbol? GetMember(this INamedTypeSymbol type, string name, params ISymbol[] args)
    {
        foreach (var member in type.GetMembers(name))
        {
            if (member is IPropertySymbol property && property.Parameters.Length == args.Length)
            {
                var match = true;

                for (int i = 0; i < args.Length; i++)
                {
                    match &= SymbolEqualityComparer.Default.Equals(args[i], property.Parameters[i].Type);
                }

                if (match)
                {
                    return property;
                }
            }
            else if (member is IMethodSymbol method && method.Parameters.Length == args.Length)
            {
                var match = true;

                for (int i = 0; i < args.Length; i++)
                {
                    match &= SymbolEqualityComparer.Default.Equals(args[i], method.Parameters[i].Type);
                }

                if (match)
                {
                    return method;
                }
            }
        }

        return null;
    }

}
