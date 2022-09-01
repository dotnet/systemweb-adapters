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
                        if (!symbols.IsSupported(invocation.TargetMethod, property.Member))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(), property.Member.Name));
                        }
                    }
                    else if (context.Operation is IPropertyReferenceOperation indexer && GetMember(indexer.Instance) is { } member)
                    {
                        if (!symbols.IsSupported(indexer.Member, member))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(), member.Name));
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
            private readonly HashSet<ISymbol?> _members;
            private readonly HashSet<ISymbol?> _unsupported;
            private readonly HashSet<ISymbol?> _allKeysUnsupported;
            private readonly HashSet<ISymbol?> _additionalUnsupported;

            public KnownSymbols(Compilation compilation)
            {
                _members = new HashSet<ISymbol?>();
                _unsupported = new HashSet<ISymbol?>(SymbolEqualityComparer.Default);
                _allKeysUnsupported = new HashSet<ISymbol?>(SymbolEqualityComparer.Default);
                _additionalUnsupported = new HashSet<ISymbol?>(SymbolEqualityComparer.Default);

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

                _members.Add(request.GetMember("Headers"));
                _members.Add(request.GetMember("Form"));
                _members.Add(request.GetMember("Cookies"));
                _members.Add(request.GetMember("QueryString"));

                _allKeysUnsupported.Add(request.GetMember("ServerVariables"));
                _allKeysUnsupported.Add(request.GetMember("Params"));

                _unsupported.Add(NameValueCollection.GetMember("Get", int32));
                _unsupported.Add(NameValueCollection.GetMember("GetKey", int32));
                _unsupported.Add(NameValueCollection.GetMember("GetValues", int32));
                _unsupported.Add(NameValueCollection.GetMember("Keys", int32));
                _unsupported.Add(NameValueCollection.GetMember("this[]", int32));

                _additionalUnsupported.Add(NameValueCollection.GetMember("AllKeys"));
                _additionalUnsupported.Add(NameValueCollection.GetMember("Count"));

                return true;
            }

            public bool IsSupported(ISymbol nameValueSymbol, ISymbol requestSymbol)
            {
                var unsupportedRequestSymbol = _members.Contains(requestSymbol) || _allKeysUnsupported.Contains(requestSymbol);

                if (unsupportedRequestSymbol && _unsupported.Contains(nameValueSymbol))
                {
                    return false;
                }

                return true;
            }

            public bool IsValid { get; }
        }
    }
}
