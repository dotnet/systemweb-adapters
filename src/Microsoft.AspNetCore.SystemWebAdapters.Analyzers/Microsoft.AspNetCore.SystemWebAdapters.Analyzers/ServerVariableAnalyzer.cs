using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.AspNetCore.SystemWebAdapters.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ServerVariableAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "SYSWEB001";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(context =>
            {
                if (context.Compilation.GetTypeByMetadataName("System.Web.HttpRequest") is not INamedTypeSymbol request)
                {
                    return;
                }

                if (context.Compilation.GetTypeByMetadataName("System.Collections.Specialized.NameValueCollection") is not INamedTypeSymbol nameValueCollection)
                {
                    return;
                }

                if (context.Compilation.GetTypeByMetadataName("System.Int32") is not INamedTypeSymbol int32)
                {
                    return;
                }

                if (GetProperty(nameValueCollection, "this[]", int32) is { } getItem && GetProperty(request, "ServerVariables") is { } getServerVariables)
                {
                    context.RegisterOperationAction(context =>
                    {
                        if (context.Operation is IPropertyReferenceOperation indexer && indexer.Instance is IPropertyReferenceOperation property)
                        {
                            if (SymbolEqualityComparer.Default.Equals(indexer.Member, getItem) && SymbolEqualityComparer.Default.Equals(property.Member, getServerVariables))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation()));
                            }
                        }

                    }, OperationKind.Invocation, OperationKind.MethodReference, OperationKind.PropertyReference);
                }
            });
        }

        private static IPropertySymbol? GetProperty(INamedTypeSymbol type, string name, params ISymbol[] args)
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
            }

            return null;
        }
    }
}
