using System.Collections;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Claims;
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
using Microsoft.Owin.Security.Interop;
using MvcApp;
using MvcApp.Models;
using Owin;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder();

var sampleMode = Enum.Parse<SampleMode>(builder.Configuration.GetValue<string>("SAMPLE_MODE"), true);

builder.AddServiceDefaults();
builder.AddSystemWebAdapters();
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
else if (sampleMode == SampleMode.Owin)
{
    builder.Services.AddAuthentication()
        .AddOwinAuthentication("SharedCookie", (app, services) =>
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);
            app.UseStageMarker(PipelineStage.Authenticate);
            var dataProtector = services.GetDataProtector(
                "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
                // Must match the Scheme name on the MvcCoreApp, i.e. IdentityConstants.ApplicationScheme
                "SharedCookie",
                "v2");

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = Microsoft.AspNet.Identity.Owin.SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                          validateInterval: TimeSpan.FromMinutes(30),
                          regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                },

                // Settings to configure shared cookie with MvcCoreApp
                CookieName = ".AspNet.ApplicationCookie",
                TicketDataFormat = new AspNetTicketDataFormat(new DataProtectorShim(dataProtector))
            });
        });
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

app.UseAuthenticationEvents();
app.UseAuthentication();
app.UseAuthorizationEvents();
app.UseAuthorization();

app.UseSystemWebAdapters();

app.MapDefaultControllerRoute();

app.MapRemoteAppFallback()
    .ShortCircuit();

app.MapDefaultEndpoints();

app.Run();
