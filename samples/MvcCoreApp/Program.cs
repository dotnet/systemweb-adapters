using ClassLibrary;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Authentication;
using Microsoft.EntityFrameworkCore;
using MvcApp.Models;

var builder = WebApplication.CreateBuilder();
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add services to the container.
builder.Services.AddControllersWithViews();

/*
// Add Identity services to migrate over Identity usage
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
*/

builder.Services.AddSystemWebAdapters()
    .AddRemoteAppSession(options =>
        {
            options.RemoteApp = new(builder.Configuration["ReverseProxy:Clusters:fallbackCluster:Destinations:fallbackApp:Address"]);
            options.ApiKey = SessionUtils.ApiKey;

            SessionUtils.RegisterSessionKeys(options);
        })
    /*
    .ConfigureSharedIdentityAuthentication(options =>
        {
            options.LoginPath = new PathString("/Account/Login");
        },
        new SharedAuthCookieOptions(SharedAuthUtils.ApplicationName, WellKnownAuthenticationSchemes.IdentityApplication),
        new SharedDirectoryDataProtectorFactory(SharedAuthUtils.SharedAuthDataProtectionDir));
    */
    .AddSharedCookieAuthentication(
        authenticaitonOptions => authenticaitonOptions.DefaultScheme = WellKnownAuthenticationSchemes.IdentityApplication,
        null,
        new SharedAuthCookieOptions(SharedAuthUtils.ApplicationName, WellKnownAuthenticationSchemes.IdentityApplication),
        new SharedDirectoryDataProtectorFactory(SharedAuthUtils.SharedAuthDataProtectionDir));

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
app.UseAuthorization();

app.UseSystemWebAdapters();

app.UseEndpoints(endpoints =>
{
    app.MapDefaultControllerRoute();
        // This method can be used to enable session (or read-only session) on all controllers
        //.RequireSystemWebAdapterSession();

    // Enabled for auth endpoints (/account/login, etc.)
    // app.MapRazorPages();

    // Fall back to ASP.NET app
    app.MapReverseProxy();
});

app.Run();
