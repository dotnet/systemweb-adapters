// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web;

internal static class Constants
{
    internal const string NotImplemented = "Not implemented yet for ASP.NET Core";

    internal const string ApiFromAspNet = "API is required to be the same as ASP.NET Framework";

    internal const string DisposeIsRegistered = "Object is registered for dispose with HttpContext";

    internal const string CA1859 = "False positive fixed by https://github.com/dotnet/roslyn-analyzers/pull/6421 but not integrated in yet";

    internal static class TransferRequest
    {
        public const string Message = "TransferRequest is not supported on ASP.NET Core";
        public const string DiagnosticId = "SYSWEB0001";
    }

    internal static class Execute
    {
        public const string Message = "Execute is not supported on ASP.NET Core";
        public const string DiagnosticId = "SYSWEB0002";
    }

    internal static class Transfer
    {
        public const string Message = "Transfer is not supported on ASP.NET Core";
        public const string DiagnosticId = "SYSWEB0003";
    }

    public static class ExperimentalFeatures
    {
        public const string DiagnosticId = "SYSWEB1001";
    }
}
