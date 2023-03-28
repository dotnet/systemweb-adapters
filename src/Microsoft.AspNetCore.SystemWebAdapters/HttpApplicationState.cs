// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace System.Web;

[SuppressMessage("Design", "CA1010:Generic interface should also be implemented", Justification = Constants.ApiFromAspNet)]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "This is internally disposed")]
public sealed class HttpApplicationState : NameObjectCollectionBase
{
    private readonly ReaderWriterLockSlim _lock = new();

    internal HttpApplicationState()
    {
    }

    public override int Count
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return base.Count;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    /// <summary>
    /// Adds a new state object to the application state collection.
    /// </summary>
    public void Add(string name, object value)
    {
        _lock.EnterWriteLock();
        try
        {
            BaseAdd(name, value);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Updates an HttpApplicationState value within the collection.
    /// </summary>
    public void Set(string name, object value)
    {
        _lock.EnterWriteLock();
        try
        {
            BaseSet(name, value);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Removes an object from the application state collection by name.
    /// </summary>
    public void Remove(string name)
    {
        _lock.EnterWriteLock();
        try
        {
            BaseRemove(name);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Removes an object from the application state collection by name.
    /// </summary>
    public void RemoveAt(int index)
    {
        _lock.EnterWriteLock();
        try
        {
            BaseRemoveAt(index);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Removes all objects from the application state collection.
    /// </summary>
    public void Clear()
    {
        _lock.EnterWriteLock();
        try
        {
            BaseClear();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Removes all objects from the application state collection.
    /// </summary>
    public void RemoveAll() => Clear();

    /// <summary>
    /// Gets an application state object by name.
    /// </summary>
    public object? Get(string name)
    {
        _lock.EnterReadLock();
        try
        {
            return BaseGet(name);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }


    /// <summary>
    /// Gets or sets a single application state object.</para>
    /// </summary>
    [DisallowNull]
    public object? this[string name]
    {
        get { return Get(name); }
        set { Set(name, value); }
    }

    /// <summary>
    /// Gets a single application state object by index.
    /// </summary>
    public object? Get(int index)
    {
        _lock.EnterReadLock();
        try
        {
            return BaseGet(index);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Gets an application state object name by index.
    /// </summary>
    public string? GetKey(int index)
    {
        _lock.EnterReadLock();
        try
        {
            return BaseGetKey(index);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }


    /// <summary>
    /// Gets an application state object by index.
    /// </summary>
    public object? this[int index] => Get(index);

    /// <summary>
    /// Gets all application state object names in collection.
    /// </summary>
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = Constants.ApiFromAspNet)]
    public string?[]? AllKeys
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return BaseGetAllKeys();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    /// <summary>
    /// Locks access to all application state variables. Facilitates access synchronization.
    /// </summary>
    public void Lock() => _lock.EnterWriteLock();

    /// <summary>
    /// Unocks access to all application state variables. Facilitates access synchronization.
    /// </summary>
    public void UnLock() => _lock.ExitWriteLock();

    /// <summary>
    /// HttpApplication does not implement IDisposable since that wouldn't work for a .NET Standard build
    /// </summary>
    internal void Dispose()
    {
        _lock.Dispose();
    }
}
