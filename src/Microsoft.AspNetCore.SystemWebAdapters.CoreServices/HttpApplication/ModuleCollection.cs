// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web;

using static System.FormattableString;

namespace Microsoft.AspNetCore.SystemWebAdapters;

/// <summary>
/// A collection that validates that the types added are actual IHttpModule types
/// </summary>
internal sealed class ModuleCollection : IDictionary<string, Type>, IModuleRegistrar
{
    private readonly Dictionary<string, Type> _inner;

    public ModuleCollection()
    {
        _inner = new(StringComparer.InvariantCultureIgnoreCase);
    }

    public Type this[string key]
    {
        get => _inner[key];
        set => _inner[key] = value;
    }

    public ICollection<string> Keys => _inner.Keys;

    public ICollection<Type> Values => _inner.Values;

    public int Count => _inner.Count;

    public bool IsReadOnly { get; private set; }

    public void Add(string key, Type type)
    {
        CheckIsReadOnly();

        if (Contains(new(key, type)))
        {
            throw new InvalidOperationException($"Module {type.FullName} is already registered with key '{key}'.");
        }

        if (!type.IsAssignableTo(typeof(IHttpModule)))
        {
            throw new InvalidOperationException($"Type {type.FullName} is not a valid IHttpModule.");
        }

        _inner.Add(key, type);
    }

    public void Add(KeyValuePair<string, Type> item) => Add(item.Key, item.Value);

    public void Clear()
    {
        CheckIsReadOnly();
        _inner.Clear();
    }

    public bool Contains(KeyValuePair<string, Type> item) => ((IDictionary<string, Type>)_inner).Contains(item);

    public bool ContainsKey(string key) => _inner.ContainsKey(key);

    public void CopyTo(KeyValuePair<string, Type>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string, Type>>)_inner).CopyTo(array, arrayIndex);

    public IEnumerator<KeyValuePair<string, Type>> GetEnumerator() => ((IEnumerable<KeyValuePair<string, Type>>)_inner).GetEnumerator();

    public void MakeReadOnly()
    {
        IsReadOnly = true;
    }

    public bool Remove(string key)
    {
        CheckIsReadOnly();
        return _inner.Remove(key);
    }

    public bool Remove(KeyValuePair<string, Type> item)
    {
        CheckIsReadOnly();
        return ((ICollection<KeyValuePair<string, Type>>)_inner).Remove(item);
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out Type value) => _inner.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();

    public void CheckIsReadOnly()
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException("Module collection is readonly");
        }
    }

    public void RegisterModule(Type type, string? name = null)
    {
        Add(name ?? MakeUniqueModuleName(type), type);

        // Gets a dynamic name similar to how ASP.NET Framework did in the static HttpApplication.RegisterModule(Type moduleType) method
        static string MakeUniqueModuleName(Type type)
            => Invariant($"__DynamicModule_{type.AssemblyQualifiedName}_{Guid.NewGuid()}");
    }
}
