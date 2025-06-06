using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Features;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddReverseProxy();

builder.Services.AddSystemWebAdapters()
    .AddSessionSerializer(options =>
    {
        options.ThrowOnUnknownSessionKey = false;
    })
    .AddJsonSessionSerializer(options =>
    {
        options.RegisterKey<int>("CoreCount");
    })
    .AddRemoteAppClient(options =>
    {
        options.ApiKey = builder.Configuration["RemoteApp:ApiKey"]!;
        options.RemoteAppUrl = new(builder.Configuration["RemoteApp:Url"]);
    })
    .AddSessionClient();

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

// Configure the the reverse proxy to forward all unhandled requests to the remote app
app.MapForwarder("/{**catch-all}", app.Configuration["RemoteApp:Url"]!)

    // If there is a route locally, we want to ensure that is used by default, but otherwise we'll forward
    .WithOrder(int.MaxValue)

    // If we're going to forward the request, there is no need to run any of the middleware after routing
    .ShortCircuit();

app.Run();
