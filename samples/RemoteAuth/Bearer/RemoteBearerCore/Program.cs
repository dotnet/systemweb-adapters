using System.Security.Claims;
using Microsoft.AspNetCore.SystemWebAdapters;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddControllers();

builder.Services.AddSystemWebAdapters()
    .AddRemoteAppClient(options =>
    {
        options.RemoteAppUrl = new(builder.Configuration["ProxyTo"]!);
        options.ApiKey = builder.Configuration["RemoteAppApiKey"]!;
    })

    // This registers the remote app authentication handler. The boolean argument indicates whether remote app auth
    // should be the default scheme. If it is set to false, HTTP requests to authenticate will only be made for
    // endpoints that actually need that behavior, but it is then necessary to annotate endpoints requiring remote app
    // auth with [Authorize(AuthenticationSchemes = RemoteAppAuthenticationDefaults.AuthenticationScheme)] or something similar.
    .AddAuthenticationClient(isDefaultScheme: true);

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/core", (ClaimsPrincipal user) => user is { } ? new
{
    Name = user.Identity?.Name,
    Claims = user.Claims.Select(c => new
    {
        c.Type,
        c.Value,
    })
} : null)
    .RequireAuthorization();

app.MapForwarder("/{**catch-all}", app.Configuration["ProxyTo"]!).WithOrder(int.MaxValue);

app.Run();
