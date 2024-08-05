using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.SystemWebAdapters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

var app = builder.Build();

app.UseAuthentication();

app.MapGet("/", (HttpContext context) =>
{
    return new
    {
        User = context.User.Identity?.Name,
        Principal = context.AsSystemWeb().User.Identity?.Name,
        LogonUser = OperatingSystem.IsWindows() ? context.AsSystemWeb().Request.LogonUserIdentity?.Name : null,
    };
});

app.Run();
