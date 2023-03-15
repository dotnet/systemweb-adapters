// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Collections.Specialized;

namespace System.Web;

[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Generic interface should also be implemented", Justification = Constants.ApiFromAspNet)]
public sealed class HttpModuleCollection : NameObjectCollectionBase
{
    internal static HttpModuleCollection Empty { get; } = new();

    private IHttpModule?[]? _all;
    private string?[]? _allKeys;

    internal HttpModuleCollection()
        : base(StringComparer.InvariantCultureIgnoreCase)
    {
        IsReadOnly = true;
    }

    internal HttpModuleCollection(IEnumerable<(string Key, IHttpModule Module)> modules)
        : base(StringComparer.InvariantCultureIgnoreCase)
    {
        foreach (var (name, module) in modules)
        {
            BaseAdd(name, module);
        }

        IsReadOnly = true;
    }

    public void CopyTo(Array dest, int index)
    {
        if (_all is null)
        {
            var n = Count;
            _all = new IHttpModule?[n];

            for (var i = 0; i < n; i++)
            {
                _all[i] = Get(i);
            }
        }

        _all.CopyTo(dest, index);
    }

    internal IEnumerable<IHttpModule> Modules
    {
        get
        {
            for (var i = 0; i < Count; i++)
            {
                if (Get(i) is IHttpModule module)
                {
                    yield return module;
                }
            }
        }
    }

    public IHttpModule? Get(string name) => (IHttpModule?)BaseGet(name);

    public IHttpModule? this[string name] => Get(name);

    public IHttpModule? Get(int index) => (IHttpModule?)BaseGet(index);

    public string? GetKey(int index) => BaseGetKey(index);

    public IHttpModule? this[int index] => Get(index);

    [Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = Constants.ApiFromAspNet)]
    public string?[] AllKeys => _allKeys ??= BaseGetAllKeys();
}
