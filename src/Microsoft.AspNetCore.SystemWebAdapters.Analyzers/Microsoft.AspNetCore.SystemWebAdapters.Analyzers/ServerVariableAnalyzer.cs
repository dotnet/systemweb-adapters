using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.AspNetCore.SystemWebAdapters.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NameValueCollectionAnalyzer : DiagnosticAnalyzer
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
                var symbols = new KnownSymbols(context.Compilation);

                if (!symbols.IsValid)
                {
                    return;
                }

                context.RegisterOperationAction(context =>
                {
                    if (context.Operation is IInvocationOperation invocation && invocation.Instance is IPropertyReferenceOperation property)
                    {
                        if (symbols.IsUnsupportedMethod(invocation.TargetMethod) && symbols.IsKnownRequestMethod(property.Member))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation()));
                        }
                    }
                    else if (context.Operation is IPropertyReferenceOperation indexer && GetMember(indexer.Instance) is { } member)
                    {
                        if (symbols.IsUnsupportedMethod(indexer.Member) && symbols.IsKnownRequestMethod(member))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation()));
                        }
                    }
                }, OperationKind.PropertyReference, OperationKind.Invocation);
            });
        }

        private static ISymbol? GetMember(IOperation operation) => operation switch
        {
            IPropertyReferenceOperation property => property.Member,
            IMethodReferenceOperation method => method.Member,
            _ => null,
        };


        private class KnownSymbols
        {
            private readonly List<ISymbol> _members;
            private readonly HashSet<ISymbol> _unsupported;

            public KnownSymbols(Compilation compilation)
            {
                _members = new List<ISymbol>();
                _unsupported = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
                IsValid = Initialize(compilation);
            }

            private bool Initialize(Compilation compilation)
            {
                if (compilation.GetTypeByMetadataName("System.Web.HttpRequest") is not { } request)
                {
                    return false;
                }

                if (compilation.GetTypeByMetadataName("System.Collections.Specialized.NameValueCollection") is not { } NameValueCollection)
                {
                    return false;
                }

                if (compilation.GetTypeByMetadataName("System.Int32") is not { } int32)
                {
                    return false;
                }

                if (compilation.GetTypeByMetadataName("System.String") is not { } stringSymbol)
                {
                    return false;
                }

                TryAdd(_members, request.GetMember("Headers"));
                TryAdd(_members, request.GetMember("Form"));
                TryAdd(_members, request.GetMember("Cookies"));
                TryAdd(_members, request.GetMember("ServerVariables"));
                TryAdd(_members, request.GetMember("QueryString"));
                TryAdd(_members, request.GetMember("Params"));

                TryAdd(_unsupported, NameValueCollection.GetMember("Get", int32));
                TryAdd(_unsupported, NameValueCollection.GetMember("GetKey", int32));
                TryAdd(_unsupported, NameValueCollection.GetMember("GetValues", int32));
                TryAdd(_unsupported, NameValueCollection.GetMember("Keys", int32));
                TryAdd(_unsupported, NameValueCollection.GetMember("this[]", int32));

                return true;

                static void TryAdd(ICollection<ISymbol> set, ISymbol? symbol)
                {
                    if (symbol is not null)
                    {
                        set.Add(symbol);
                    }
                }
            }

            public bool IsKnownRequestMethod(ISymbol symbol) => _members.Contains(symbol);

            public bool IsUnsupportedMethod(ISymbol symbol) => _unsupported.Contains(symbol);

            public bool IsValid { get; }
        }
    }
}
