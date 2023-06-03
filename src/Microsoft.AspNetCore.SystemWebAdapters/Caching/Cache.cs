// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Runtime.Caching;

namespace System.Web.Caching;

public delegate void CacheItemRemovedCallback(string key, object value, CacheItemRemovedReason reason);
public delegate void CacheItemUpdateCallback(string key, CacheItemUpdateReason reason, out object? expensiveObject, out CacheDependency? dependency, out DateTime absoluteExpiration, out TimeSpan slidingExpiration);

[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Generic interface should also be implemented", Justification = Constants.ApiFromAspNet)]
[Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = Constants.ApiFromAspNet)]
[Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1724:Type names should not match namespaces", Justification = Constants.ApiFromAspNet)]
public sealed class Cache : IEnumerable
{
    private readonly ObjectCache _cache;

    public static readonly DateTime NoAbsoluteExpiration = DateTime.MaxValue;

    public static readonly TimeSpan NoSlidingExpiration = TimeSpan.Zero;

    public Cache()
        : this(MemoryCache.Default)
    {
    }

    public Cache(ObjectCache cache)
    {
        _cache = cache;
    }

    public object this[string key]
    {
        get => Get(key);
        set => Insert(key, value);
    }

    public object Add(string key, object value, CacheDependency? dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback? onRemoveCallback)
    {
        var policy = new CacheItemPolicy
        {
            AbsoluteExpiration = Convert(absoluteExpiration),
            SlidingExpiration = slidingExpiration,
            Priority = Convert(priority),
            RemovedCallback = Convert(onRemoveCallback),
        };
        AddChangeMonitors(dependencies, policy);

        return _cache.AddOrGetExisting(key, value, policy);
    }

    private static void AddChangeMonitors(CacheDependency? dependencies, CacheItemPolicy policy)
    {
        if (dependencies?.ChangeMonitors is not null)
        {
            policy.ChangeMonitors.Add(dependencies.GetChangeMonitor());
        }
    }

    public object Get(string key) => _cache.Get(key);

    public void Insert(string key, object value) => _cache.Set(key, value, new CacheItemPolicy());

    public void Insert(string key, object value, CacheDependency? dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration)
    {
        var policy = new CacheItemPolicy
        {
            AbsoluteExpiration = Convert(absoluteExpiration),
            SlidingExpiration = slidingExpiration,
        };
        AddChangeMonitors(dependencies, policy);

        _cache.Set(key, value, policy);
    }

    public void Insert(string key, object value, CacheDependency? dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback? onRemoveCallback)
    {
        var policy = new CacheItemPolicy
        {
            AbsoluteExpiration = Convert(absoluteExpiration),
            SlidingExpiration = slidingExpiration,
            Priority = Convert(priority),
            RemovedCallback = Convert(onRemoveCallback),
        };
        AddChangeMonitors(dependencies, policy);

        _cache.Set(key, value, policy);
    }

    public void Insert(string key, object value, CacheDependency? dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemUpdateCallback onUpdateCallback)
    {
        var policy = new CacheItemPolicy
        {
            AbsoluteExpiration = Convert(absoluteExpiration),
            SlidingExpiration = slidingExpiration,
            UpdateCallback = Convert(onUpdateCallback),
        };
        AddChangeMonitors(dependencies, policy);

        _cache.Set(key, value, policy);
    }

    public object? Remove(string key) => _cache.Remove(key);

    private static Runtime.Caching.CacheItemPriority Convert(CacheItemPriority priority) => priority switch
    {
        CacheItemPriority.NotRemovable => Runtime.Caching.CacheItemPriority.NotRemovable,
        _ => Runtime.Caching.CacheItemPriority.Default,
    };

    private static CacheItemRemovedReason Convert(CacheEntryRemovedReason reason) => reason switch
    {
        CacheEntryRemovedReason.Expired => CacheItemRemovedReason.Expired,
        CacheEntryRemovedReason.Evicted => CacheItemRemovedReason.Underused,
        CacheEntryRemovedReason.ChangeMonitorChanged => CacheItemRemovedReason.DependencyChanged,
        _ => CacheItemRemovedReason.Removed,
    };

    private static CacheEntryRemovedCallback? Convert(CacheItemRemovedCallback? callback)
    {
        if (callback is null)
        {
            return null;
        }

        return args =>
        {
            if (args.CacheItem is null)
            {
                return;
            }

            callback(args.CacheItem.Key, args.CacheItem.Value, Convert(args.RemovedReason));
        };
    }

    private static DateTimeOffset Convert(DateTime dt) => dt == NoAbsoluteExpiration ? DateTimeOffset.MaxValue : dt;

    private static CacheEntryUpdateCallback? Convert(CacheItemUpdateCallback? callback)
    {
        if (callback is null)
        {
            return null;
        }

        return args =>
        {
            var reason = args.RemovedReason switch
            {
                CacheEntryRemovedReason.ChangeMonitorChanged => CacheItemUpdateReason.DependencyChanged,
                _ => CacheItemUpdateReason.Expired,
            };

            callback(args.Key, reason, out var expensiveObject, out _, out var absoluteExpiration, out var slidingExpiration);

            if (expensiveObject is null)
            {
                return;
            }

            args.UpdatedCacheItem = new(args.Key, expensiveObject);
            args.UpdatedCacheItemPolicy = new()
            {
                SlidingExpiration = slidingExpiration,
                AbsoluteExpiration = Convert(absoluteExpiration),
                UpdateCallback = Convert(callback),
            };
        };
    }

    public int Count => (int)_cache.GetCount();

    internal ObjectCache ObjectCache => _cache;

    public IEnumerator GetEnumerator() => ((IEnumerable)_cache).GetEnumerator();
}
