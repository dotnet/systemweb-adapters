using System.Runtime.Caching;

namespace System.Web.Caching;

internal static class CacheExtensions
{
    public static CacheItemPolicy WithCacheDependency(this CacheItemPolicy policy, CacheDependency? dependency)
    {
        if (dependency is not null)
        {
            policy.ChangeMonitors.Add(dependency.GetChangeMonitor());
        }
        return policy;
    }

    public static void Set(this ObjectCache cache, string key, object value) =>
        cache.Set(key, value, new CacheItemPolicy());

    public static void Set(this ObjectCache cache, string key, object value,
        CacheDependency? dependency) =>
        cache.Set(key, value, new CacheItemPolicy().WithCacheDependency(dependency));

    public static void Set(this ObjectCache cache, string key, object value,
        CacheItemPolicy policy, CacheDependency? dependency) =>
        cache.Set(key, value, policy.WithCacheDependency(dependency));

    public static object AddOrGetExisting(this ObjectCache cache, string key, object value,
        CacheItemPolicy policy, CacheDependency? dependency) =>
        cache.AddOrGetExisting(key, value, policy.WithCacheDependency(dependency));

}
