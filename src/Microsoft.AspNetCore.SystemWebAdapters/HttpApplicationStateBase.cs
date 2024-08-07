// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace System.Web;

[SuppressMessage("Design", "CA1010:Generic interface should also be implemented", Justification = Constants.ApiFromAspNet)]
[SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = Constants.ApiFromAspNet)]
public abstract class HttpApplicationStateBase : NameObjectCollectionBase, ICollection
{
    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = Constants.ApiFromAspNet)]
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = Constants.ApiFromAspNet)]
    public virtual string?[]? AllKeys => throw new NotImplementedException();

    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = Constants.ApiFromAspNet)]
    public virtual HttpApplicationStateBase Contents => throw new NotImplementedException();

    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = Constants.ApiFromAspNet)]
    public override int Count => throw new NotImplementedException();

    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = Constants.ApiFromAspNet)]
    public virtual bool IsSynchronized => throw new NotImplementedException();

    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = Constants.ApiFromAspNet)]
    public virtual object SyncRoot => throw new NotImplementedException();

    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = Constants.ApiFromAspNet)]
    [DisallowNull]
    public virtual object? this[int index] => throw new NotImplementedException();

    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = Constants.ApiFromAspNet)]
    [DisallowNull]
    public virtual object? this[string name]
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public virtual void Add(string name, object value)
    {
        throw new NotImplementedException();
    }

    public virtual void Clear()
    {
        throw new NotImplementedException();
    }

    public virtual void CopyTo(Array array, int index)
    {
        throw new NotImplementedException();
    }

    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = Constants.ApiFromAspNet)]
    public virtual object? Get(int index)
    {
        throw new NotImplementedException();
    }

    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = Constants.ApiFromAspNet)]
    public virtual object? Get(string name)
    {
        throw new NotImplementedException();
    }

    public override IEnumerator GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public virtual string? GetKey(int index)
    {
        throw new NotImplementedException();
    }

    public virtual void Lock()
    {
        throw new NotImplementedException();
    }

    public virtual void Remove(string name)
    {
        throw new NotImplementedException();
    }

    public virtual void RemoveAll()
    {
        throw new NotImplementedException();
    }

    public virtual void RemoveAt(int index)
    {
        throw new NotImplementedException();
    }

    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = Constants.ApiFromAspNet)]
    public virtual void Set(string name, object value)
    {
        throw new NotImplementedException();
    }

    public virtual void UnLock()
    {
        throw new NotImplementedException();
    }
}
