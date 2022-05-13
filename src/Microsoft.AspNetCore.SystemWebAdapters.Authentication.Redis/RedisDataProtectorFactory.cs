using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

/// <summary>
/// Creates IDataProtectors that can be used to store keys for
/// protecting and unprotecting auth cookies in a Redis cache.
/// </summary>
public sealed class RedisDataProtectorFactory : ICookieDataProtectorFactory, IDisposable
{
    private readonly ConnectionMultiplexer _redisMux;

    public RedisDataProtectorFactory(string redisUri) : this(ConnectionMultiplexer.Connect(redisUri))
    {
    }

    public RedisDataProtectorFactory(ConnectionMultiplexer redisConnectionMux)
    {
        _redisMux = redisConnectionMux;
    }

    public IDataProtectionProvider CreateDataProtectionProvider(SharedAuthCookieOptions options)
    {
        var services = new ServiceCollection();
        services.AddDataProtection()
            .SetApplicationName(options.ApplicationName)
            .PersistKeysToStackExchangeRedis(_redisMux);

        using var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IDataProtectionProvider>();
    }

    public void Dispose() => _redisMux?.Dispose();
}
