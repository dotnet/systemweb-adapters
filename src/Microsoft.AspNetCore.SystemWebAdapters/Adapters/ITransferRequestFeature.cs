// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

using System.Collections.Specialized;
using System.IO;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal interface ITransferRequestFeature
{
    void Transfer(string path, bool preserveForm) => Execute(path, null, preserveForm);

    void Execute(string path, TextWriter? writer, bool preserveForm);

    void TransferRequest(string path, bool preserveForm, string? method, NameValueCollection? headers, bool preserveUser);
}

#endif
