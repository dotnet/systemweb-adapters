using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

using HttpContext = System.Web.HttpContext;
using HttpContextCore = Microsoft.AspNetCore.Http.HttpContext;

internal static class SessionExampleExtensions
{
    private const string SessionKey = "array";

    public static ISystemWebAdapterBuilder AddCustomSerialization(this ISystemWebAdapterBuilder builder)
    {
        builder.Services.AddSingleton<ISessionKeySerializer>(new ByteArraySerializer(SessionKey));
        return builder;
    }

    public static void MapSessionExample(this RouteGroupBuilder builder)
    {
        builder.RequireSystemWebAdapterSession();

        builder.MapGet("/custom", (HttpContextCore ctx) =>
        {
            return GetValue(ctx);

            static object? GetValue(HttpContext context)
                => context.Session?[SessionKey];
        });

        builder.MapPost("/custom", async (HttpContextCore ctx) =>
        {
            using var ms = new MemoryStream();
            await ctx.Request.Body.CopyToAsync(ms);

            SetValue(ctx, ms.ToArray());

            static void SetValue(HttpContext context, byte[] data)
                => context.Session![SessionKey] = data;
        });

        builder.MapGet("/count", (HttpContextCore ctx) =>
        {
            var context = (HttpContext)ctx;

            if (context.Session!["callCount"] is not int count)
            {
                count = 0;
            }

            context.Session!["callCount"] = ++count;

            return $"This endpoint has been hit {count} time(s) this session";
        });
    }

    /// <summary>
    /// This is an example of a custom <see cref="ISessionKeySerializer"/> that takes a key name and expectes the value to be a byte array.
    /// This shows how to implement a custom serializer, which can then be registered in the DI container. When the session middleware
    /// attempts to serialize/deserialize keys, it will cycle through all the registered <see cref="ISessionKeySerializer"/> instances until
    /// it finds one that can handle the given key.
    /// </summary>
    private sealed class ByteArraySerializer : ISessionKeySerializer
    {
        private readonly string _key;

        public ByteArraySerializer(string key)
        {
            _key = key;
        }

        public bool TryDeserialize(string key, byte[] bytes, out object? obj)
        {
            if (string.Equals(_key, key, StringComparison.Ordinal))
            {
                obj = bytes;
                return true;
            }

            obj = null;
            return false;
        }

        public bool TrySerialize(string key, object value, out byte[] bytes)
        {
            if (string.Equals(_key, key, StringComparison.Ordinal) && value is byte[] valueBytes)
            {
                bytes = valueBytes;
                return true;
            }

            bytes = Array.Empty<byte>();
            return false;
        }
    }
}
