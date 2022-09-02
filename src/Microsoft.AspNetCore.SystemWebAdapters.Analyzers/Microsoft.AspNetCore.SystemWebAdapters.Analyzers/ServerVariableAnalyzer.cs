// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.AspNetCore.SystemWebAdapters.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class NameValueCollectionAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "SYSWEB001";

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
                        if (symbols.IsSupported(invocation.TargetMethod, property.Member) is { IsSupported: false, Properties: { } p })
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(), p, property.Member.Name, invocation.TargetMethod.Name));
                        }
                    }
                    else if (context.Operation is IPropertyReferenceOperation indexer && GetMember(indexer.Instance) is { } member)
                    {
                        if (symbols.IsSupported(indexer.Member, member) is { IsSupported: false, Properties: { } p })
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(), p, member.Name, indexer.Member.Name));
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

        private readonly struct SupportDetails
        {
            public SupportDetails(bool isSupported, ImmutableDictionary<string, string> properties)
            {
                IsSupported = isSupported;
                Properties = properties;
            }

            public bool IsSupported { get; }

            public ImmutableDictionary<string, string> Properties { get; }
        }


        private class KnownSymbols
        {
            private readonly HashSet<ISymbol?> _requestMembers;
            private readonly HashSet<ISymbol?> _nameValueMembers;
            private readonly HashSet<ISymbol?> _requestMembersAdditional;
            private readonly HashSet<ISymbol?> _nameValueMembersAdditional;

            public KnownSymbols(Compilation compilation)
            {
                _requestMembers = new HashSet<ISymbol?>();
                _nameValueMembers = new HashSet<ISymbol?>(SymbolEqualityComparer.Default);
                _requestMembersAdditional = new HashSet<ISymbol?>(SymbolEqualityComparer.Default);
                _nameValueMembersAdditional = new HashSet<ISymbol?>(SymbolEqualityComparer.Default);

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

                _requestMembers.Add(request.GetMember("Headers"));
                _requestMembers.Add(request.GetMember("Form"));
                _requestMembers.Add(request.GetMember("QueryString"));

                _requestMembersAdditional.Add(request.GetMember("ServerVariables"));
                _requestMembersAdditional.Add(request.GetMember("Params"));

                _nameValueMembers.Add(NameValueCollection.GetMember("Get", int32));
                _nameValueMembers.Add(NameValueCollection.GetMember("GetKey", int32));
                _nameValueMembers.Add(NameValueCollection.GetMember("GetValues", int32));
                _nameValueMembers.Add(NameValueCollection.GetMember("Keys"));
                _nameValueMembers.Add(NameValueCollection.GetMember("GetEnumerator"));
                _nameValueMembers.Add(NameValueCollection.GetMember("this[]", int32));

                _nameValueMembersAdditional.Add(NameValueCollection.GetMember("AllKeys"));
                _nameValueMembersAdditional.Add(NameValueCollection.GetMember("Count"));

                return true;
            }

            public bool IsRequestSymbol(ISymbol requestSymbol) => _requestMembers.Contains(requestSymbol) || _requestMembersAdditional.Contains(requestSymbol);

            public SupportDetails IsSupported(ISymbol nameValueSymbol, ISymbol requestSymbol)
            {
                var allKeysUnsupported = _requestMembersAdditional.Contains(requestSymbol);
                var unsupportedRequestSymbol = allKeysUnsupported || _requestMembers.Contains(requestSymbol);

                if (unsupportedRequestSymbol && _nameValueMembers.Contains(nameValueSymbol))
                {
                    return CreateDetails(allKeysUnsupported, nameValueSymbol);
                }

                if (allKeysUnsupported && _nameValueMembersAdditional.Contains(nameValueSymbol))
                {
                    return CreateDetails(allKeysUnsupported, nameValueSymbol);
                }

                return Supported;
            }

            private SupportDetails CreateDetails(bool allKeysUnsupported, ISymbol nameValueSymbol)
            {
                if (!allKeysUnsupported && nameValueSymbol.Name is "Keys" or "GetEnumerator")
                {
                    return ReplaceWithAllKeys;
                }

                return Unsupported;
            }

            private readonly SupportDetails ReplaceWithAllKeys = new(false, ImmutableDictionary.Create<string, string>().Add("Replace", "AllKeys"));
            private readonly SupportDetails Unsupported = new(false, ImmutableDictionary.Create<string, string>());
            private readonly SupportDetails Supported = new(true, ImmutableDictionary.Create<string, string>());

            public bool IsValid { get; }
        }
    }
}
