// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;

internal interface IUnknownKeyTracker
{
    ILookup<string, Type> UnknownTypes { get; }

    IReadOnlyCollection<string> UnknownKeys { get; }

    void Add(string key, Type type);

    void Add(IEnumerable<string> keys);
}
