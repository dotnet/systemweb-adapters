// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace System.Web;

[SuppressMessage("Design", "CA1010:Generic interface should also be implemented", Justification = Constants.ApiFromAspNet)]
public class HttpApplicationStateWrapper : HttpApplicationStateBase
{
    private readonly HttpApplicationState _application;

    public HttpApplicationStateWrapper(HttpApplicationState httpApplicationState)
    {
        ArgumentNullException.ThrowIfNull(httpApplicationState);

        _application = httpApplicationState;
    }

    public override string?[]? AllKeys => _application.AllKeys;

    public override HttpApplicationStateBase Contents => this;

    public override int Count => _application.Count;

    public override bool IsSynchronized => ((ICollection)_application).IsSynchronized;

    public override NameObjectCollectionBase.KeysCollection Keys => _application.Keys;

    public override object SyncRoot => ((ICollection)_application).SyncRoot;

    [DisallowNull]
    public override object? this[int index] => _application[index]!;

    [DisallowNull]
    public override object? this[string name]
    {
        get => _application[name];
        set => _application[name] = value;
    }

    public override void Add(string name, object value)
    {
        _application.Add(name, value);
    }

    public override void Clear()
    {
        _application.Clear();
    }

    public override void CopyTo(Array array, int index)
    {
        ((ICollection)_application).CopyTo(array, index);
    }

    public override object? Get(int index)
    {
        return _application.Get(index);
    }

    public override object? Get(string name)
    {
        return _application.Get(name);
    }

    public override IEnumerator GetEnumerator()
    {
        return _application.GetEnumerator();
    }

    public override string? GetKey(int index)
    {
        return _application.GetKey(index);
    }

    public override void Lock()
    {
        _application.Lock();
    }

    public override void Remove(string name)
    {
        _application.Remove(name);
    }

    public override void RemoveAll()
    {
        _application.RemoveAll();
    }

    public override void RemoveAt(int index)
    {
        _application.RemoveAt(index);
    }

    public override void Set(string name, object value)
    {
        _application.Set(name, value);
    }

    public override void UnLock()
    {
        _application.UnLock();
    }
}
