// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

using System.Collections.Specialized;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Features;

/// <summary>
/// Represents the ability to execute a new request.
/// </summary>
[Experimental(Constants.ExperimentalFeatures.DiagnosticId)]
interface ITransferRequestFeature
{
    void Transfer(string path, bool preserveForm) => Execute(path, null, preserveForm);

    void Execute(string path, TextWriter? writer, bool preserveForm);

    void TransferRequest(string path, bool preserveForm, string? method, NameValueCollection? headers, bool preserveUser);
}

#endif
