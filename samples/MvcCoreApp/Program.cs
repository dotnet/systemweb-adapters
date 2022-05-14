using ClassLibrary;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Authentication;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder();
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add services to the container.
builder.Services.AddControllersWithViews();

#if DUAL_IDENTITY
// Add Identity services to migrate over Identity usage
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Specify DataDirectory for use in connection string
// This is only needed for demo purposes since the two apps need to share an on-disk mdf-based SQL database
AppDomain.CurrentDomain.SetData("DataDirectory", SharedAuthUtils.SharedAuthDataProtectionDir.FullName);
#endif

builder.Services.AddSystemWebAdapters()
    .AddRemoteAppSession(options =>
    {
        options.RemoteApp = new(builder.Configuration["ReverseProxy:Clusters:fallbackCluster:Destinations:fallbackApp:Address"]);
        options.ApiKey = SessionUtils.ApiKey;

        SessionUtils.RegisterSessionKeys(options);
    })
#if DUAL_IDENTITY
    .ConfigureSharedIdentityAuthentication(options =>
        {
            options.LoginPath = new PathString("/Account/Login");
        },
        new SharedAuthCookieOptions(SharedAuthUtils.ApplicationName, WellKnownAuthenticationSchemes.IdentityApplication),
        new SharedDirectoryDataProtectorFactory(SharedAuthUtils.SharedAuthDataProtectionDir));
#else
    .AddSharedCookieAuthentication(
        authenticaitonOptions => authenticaitonOptions.DefaultScheme = WellKnownAuthenticationSchemes.IdentityApplication,
        null,
        new SharedAuthCookieOptions(SharedAuthUtils.ApplicationName, WellKnownAuthenticationSchemes.IdentityApplication),
        new SharedDirectoryDataProtectorFactory(SharedAuthUtils.SharedAuthDataProtectionDir));
#endif

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

#if DUAL_IDENTITY
    // Enabled for auth endpoints (/account/login, etc.)
    app.MapRazorPages();
#endif

    // Fall back to ASP.NET app
    app.MapReverseProxy();
});

app.Run();
