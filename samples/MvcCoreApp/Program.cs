using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Authentication;
using Microsoft.EntityFrameworkCore;
using MvcCoreApp.Models;

var builder = WebApplication.CreateBuilder();
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add identity
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add SystemWebAdapters including remote session state
builder.Services.AddSystemWebAdapters()
    .AddRemoteAppSession(options =>
    {
        options.RemoteApp = new(builder.Configuration["ReverseProxy:Clusters:fallbackCluster:Destinations:fallbackApp:Address"]);
        options.ApiKey = ClassLibrary.SessionUtils.ApiKey;

        ClassLibrary.SessionUtils.RegisterSessionKeys(options);
    });

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
app.UseClaimsPrincipalForwarding();

app.UseEndpoints(endpoints =>
{
    app.MapDefaultControllerRoute();
        // This method can be used to enable session (or read-only session) on all controllers
        //.RequireSystemWebAdapterSession();

    // For account login/logout UI
    app.MapRazorPages();

    app.MapReverseProxy();
});

app.Run();
