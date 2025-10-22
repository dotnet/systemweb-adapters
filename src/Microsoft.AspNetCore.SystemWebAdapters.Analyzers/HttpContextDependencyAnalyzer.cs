using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.AspNetCore.SystemWebAdapters.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
public class HttpContextDependencyAnalyzer : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor s_Rule = new DiagnosticDescriptor(
        id: "SYSWEB1001",
        title: "Do not cast HttpContext or HttpContextBase to IServiceProvider",
        messageFormat: "{0} is implicitly convertable to IServiceProvider but does not return any useful services. Prefer the {0}.GetRequestServices() extension method instead.",
        category: "Error",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [s_Rule];

    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(context =>
        {
            if (context.Operation is not IConversionOperation { Type: { } type, Operand.Type: { } operand })
            {
                return;
            }

            // Allows us to fail fast for types we don't care about
            if (!operand.IsInAssembly("System.Web"))
            {
                return;
            }

            if (!context.Compilation.IsType(type, "System.IServiceProvider"))
            {
                return;
            }

            if (context.Compilation.IsType(operand, "System.Web.HttpContext") || context.Compilation.IsType(operand, "System.Web.HttpContextBase"))
            {
                context.ReportDiagnostic(Diagnostic.Create(s_Rule, context.Operation.Syntax.GetLocation(), operand.Name));
            }
        }, OperationKind.Conversion);
    }
}
