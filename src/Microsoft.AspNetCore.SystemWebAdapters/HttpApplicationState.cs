// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace System.Web;

[SuppressMessage("Design", "CA1010:Generic interface should also be implemented", Justification = Constants.ApiFromAspNet)]
public sealed class HttpApplicationState : NameObjectCollectionBase
{
    private readonly HttpApplicationStateLock _lock = new HttpApplicationStateLock();

    internal HttpApplicationState()
    {
    }

    public override int Count
    {
        get
        {
            _lock.AcquireRead();
            try
            {
                return base.Count;
            }
            finally
            {
                _lock.ReleaseRead();
            }
        }
    }

    /// <summary>
    /// Adds a new state object to the application state collection.
    /// </summary>
    public void Add(string name, object value)
    {
        _lock.AcquireWrite();
        try
        {
            BaseAdd(name, value);
        }
        finally
        {
            _lock.ReleaseWrite();
        }
    }

    /// <summary>
    /// Updates an HttpApplicationState value within the collection.
    /// </summary>
    public void Set(string name, object value)
    {
        _lock.AcquireWrite();
        try
        {
            BaseSet(name, value);
        }
        finally
        {
            _lock.ReleaseWrite();
        }
    }

    /// <summary>
    /// Removes an object from the application state collection by name.
    /// </summary>
    public void Remove(string name)
    {
        _lock.AcquireWrite();
        try
        {
            BaseRemove(name);
        }
        finally
        {
            _lock.ReleaseWrite();
        }
    }

    /// <summary>
    /// Removes an object from the application state collection by name.
    /// </summary>
    public void RemoveAt(int index)
    {
        _lock.AcquireWrite();
        try
        {
            BaseRemoveAt(index);
        }
        finally
        {
            _lock.ReleaseWrite();
        }
    }

    /// <summary>
    /// Removes all objects from the application state collection.
    /// </summary>
    public void Clear()
    {
        _lock.AcquireWrite();
        try
        {
            BaseClear();
        }
        finally
        {
            _lock.ReleaseWrite();
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
        _lock.AcquireRead();
        try
        {
            return BaseGet(name);
        }
        finally
        {
            _lock.ReleaseRead();
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
        _lock.AcquireRead();
        try
        {
            return BaseGet(index);
        }
        finally
        {
            _lock.ReleaseRead();
        }
    }

    /// <summary>
    /// Gets an application state object name by index.
    /// </summary>
    public string? GetKey(int index)
    {
        _lock.AcquireRead();
        try
        {
            return BaseGetKey(index);
        }
        finally
        {
            _lock.ReleaseRead();
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
            _lock.AcquireRead();
            try
            {
                return BaseGetAllKeys();
            }
            finally
            {
                _lock.ReleaseRead();
            }
        }
    }

    /// <summary>
    /// Locks access to all application state variables. Facilitates access synchronization.
    /// </summary>
    public void Lock() => _lock.AcquireWrite();

    /// <summary>
    /// Unocks access to all application state variables. Facilitates access synchronization.
    /// </summary>
    public void UnLock() => _lock.ReleaseWrite();

    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Should dispose but WIP")]
    private class HttpApplicationStateLock
    {
        private readonly ReaderWriterLockSlim _lock;

        internal HttpApplicationStateLock()
        {
            _lock = new ReaderWriterLockSlim();
        }

        internal void AcquireRead() => _lock.EnterReadLock();

        internal void ReleaseRead() => _lock.ExitReadLock();

        internal void AcquireWrite() => _lock.EnterWriteLock();

        internal void ReleaseWrite() => _lock.ExitWriteLock();
    }
}
