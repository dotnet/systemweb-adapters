using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Features;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddSystemWebAdapters()
    .AddSessionSerializer(options =>
    {
        options.ThrowOnUnknownSessionKey = false;
    })
    .AddJsonSessionSerializer(options =>
    {
        options.RegisterKey<int>("CoreCount");
    });

var app = builder.Build();

app.UseSystemWebAdapters();

app.Map("/", (HttpContext context) =>
{
    var session = context.AsSystemWeb().Session!;

    if (session["CoreCount"] is int count)
    {
        session["CoreCount"] = count + 1;
    }
    else
    {
        session["CoreCount"] = 1;
    }

    return session.Cast<string>().Select(key => new { Key = key, Value = session[key] });
}).RequireSystemWebAdapterSession();

app.MapRemoteAppFallback();

app.Run();
