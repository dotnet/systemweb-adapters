// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Microsoft.AspNetCore.SystemWebAdapters.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NameValueCollectionCodeFixProvider)), Shared]
public class NameValueCollectionCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(NameValueCollectionAnalyzer.DiagnosticId);

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var diagnostic = context.Diagnostics.First();

        if (!diagnostic.Properties.TryGetValue("Replace", out var newName))
        {
            return;
        }

        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the type declaration identified by the diagnostic.
        var node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);
        var newNode = node switch
        {
            MemberAccessExpressionSyntax member => (SyntaxNode)member.WithName(SyntaxFactory.IdentifierName(newName)),
            InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax member} invocation => member.WithName(SyntaxFactory.IdentifierName(newName)),
            _ => null,
        };

        if (newNode is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: CodeFixResources.CodeFixTitle,
                createChangedDocument: c => ReplaceNameAsync(context.Document, node, newNode, c),
                equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
            diagnostic);
    }

    private static async Task<Document> ReplaceNameAsync(Document document, SyntaxNode node, SyntaxNode newNode, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

        editor.ReplaceNode(node, newNode);

        return editor.GetChangedDocument();
    }
}
