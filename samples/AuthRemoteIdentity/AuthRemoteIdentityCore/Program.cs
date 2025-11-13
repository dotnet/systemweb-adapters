using System.Collections;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using AuthRemoteIdentityCore.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Interop;
using Microsoft.Owin.Security.OAuth;
using MvcApp;
using MvcApp.Models;
using Owin;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder();

var sampleMode = Enum.Parse<SampleMode>(builder.Configuration.GetValue<string>("SAMPLE_MODE"), true);

builder.AddServiceDefaults();
builder.AddSystemWebAdapters()
    .AddStaticUserAccessors();

builder.Services.AddSingleton<MatcherPolicy>(new SamplesPolicy(sampleMode));

// These must match the data protection settings in MvcApp Startup.Auth.cs for cookie sharing to work
var sharedApplicationName = "CommonMvcAppName";
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Path.GetTempPath(), "sharedkeys", sharedApplicationName)))
    .SetApplicationName(sharedApplicationName);

if (sampleMode == SampleMode.Remote)
{
    builder.Services.AddAuthentication()
        .AddCookie("SharedCookie", options => options.Cookie.Name = ".AspNet.ApplicationCookie");
}

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthenticationEvents();

if (sampleMode == SampleMode.Owin)
{
    app.UseOwin((app, services) =>
    {
        // owin auth stuff
    });
}

app.UseAuthorization();
app.UseAuthorizationEvents();

if (sampleMode == SampleMode.Owin)
{
    app.UseOwin((app, services) =>
    {
        // TODO: add in the Owin config
        //use a cookie to temporarily store information about a user logging in with a third party login provider
        app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalBearer);

        var OAuthBearerOptions = new OAuthBearerAuthenticationOptions();

        var oAuthServerOptions = new OAuthAuthorizationServerOptions
        {
            AllowInsecureHttp = true,
            TokenEndpointPath = new Microsoft.Owin.PathString("/api/v1/token"),
            Provider = new SimpleAuthorizationServerProvider(),
        };

        // Token Generation
        app.UseOAuthAuthorizationServer(oAuthServerOptions);
        app.UseOAuthBearerAuthentication(OAuthBearerOptions);
    });
}

app.UseSystemWebAdapters();

app.MapDefaultControllerRoute();

app.Map("/user", () =>
{
    var user = ClaimsPrincipal.Current;

    if (user is null)
    {
        return Results.Problem("Empty ClaimsPrincipal");
    }

    return Results.Json(new
    {
        IsAuthenticated = user.Identity?.IsAuthenticated ?? false,
        Name = user.Identity?.Name,
        Claims = user.Claims.Select(c => new { c.Type, c.Value })
    });
}).WithMetadata(new SetThreadCurrentPrincipalAttribute()); ;

//app.MapRemoteAppFallback()
//    .ShortCircuit();

app.MapDefaultEndpoints();

app.Run();
