// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class TypeCollector : SymbolVisitor
{
    private HashSet<string> Members { get; } = new HashSet<string>();

    public static HashSet<string> Collect(IAssemblySymbol assembly)
    {
        var collector = new TypeCollector();

        collector.Visit(assembly);

        return collector.Members;
    }

    public override void VisitAssembly(IAssemblySymbol symbol)
    {
        symbol.GlobalNamespace.Accept(this);

        foreach (var forwarded in symbol.GetForwardedTypes())
        {
            forwarded.Accept(this);
        }
    }

    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        foreach (var member in symbol.GetMembers())
        {
            member.Accept(this);
        }
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        if (!ShouldValidate(symbol))
        {
            return;
        }

        Members.Add(symbol.GetDocumentationCommentId()!);

        foreach (var member in symbol.GetMembers())
        {
            member.Accept(this);
        }
    }

    private static bool ShouldValidate(ISymbol symbol) => IsPublic(symbol);

    private static bool IsPublic(ISymbol symbol) => symbol.DeclaredAccessibility switch
    {
        Accessibility.Private => false,
        Accessibility.Internal => false,
        Accessibility.ProtectedAndInternal => false,
        Accessibility.Protected => true,
        Accessibility.ProtectedOrInternal => true,
        Accessibility.Public => true,
        _ => throw new NotImplementedException(),
    };

    public override void VisitMethod(IMethodSymbol symbol)
    {
        if (ShouldValidate(symbol))
        {
            Members.Add($"{symbol.ReturnType.GetDocumentationCommentId()} {symbol.GetDocumentationCommentId()}");
        }
    }

    public override void VisitProperty(IPropertySymbol symbol)
    {
        if (ShouldValidate(symbol))
        {
            Members.Add($"{symbol.Type.GetDocumentationCommentId()} {symbol.GetDocumentationCommentId()}");
        }
    }

    public override void VisitEvent(IEventSymbol symbol)
    {
        if (ShouldValidate(symbol))
        {
            Members.Add($"{symbol.Type.GetDocumentationCommentId()} {symbol.GetDocumentationCommentId()}");
        }
    }

    public override void VisitField(IFieldSymbol symbol)
    {
        if (ShouldValidate(symbol))
        {
            Members.Add($"{symbol.Type.GetDocumentationCommentId()} {symbol.GetDocumentationCommentId()}");
        }
    }
}
