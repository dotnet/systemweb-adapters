using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.SystemWebAdapters;

var builder = WebApplication.CreateBuilder();
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// These must match the data protection settings in MvcApp Startup.Auth.cs for cookie sharing to work
var sharedApplicationName = "CommonMvcAppName";
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Path.GetTempPath(), "sharedkeys", sharedApplicationName)))
    .SetApplicationName(sharedApplicationName);

builder.Services.AddAuthentication()
    .AddCookie("SharedCookie", options => options.Cookie.Name = ".AspNet.ApplicationCookie");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSystemWebAdapters()
    .AddHttpModule<MyModule>()
    .AddHttpApplication<MyApp>()
    .AddJsonSessionSerializer(options => ClassLibrary.RemoteServiceUtils.RegisterSessionKeys(options.KnownKeys));

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
app.UseRaiseAuthenticationEvents();

app.UseAuthorization();
app.UseRaiseAuthorizationEvents();

app.UseSystemWebAdapters();

app.MapGet("/current-principals-with-metadata", (HttpContext ctx) =>
{
    var user1 = Thread.CurrentPrincipal;
    var user2 = ClaimsPrincipal.Current;

    return "done";
}).WithMetadata(new SetThreadCurrentPrincipalAttribute(), new SingleThreadedRequestAttribute());

app.MapGet("/test", async () =>
{
    await Task.Delay(1000);
    return "hi";
});

app.MapGet("/current-principals-no-metadata", (HttpContext ctx) =>
{
    var user1 = Thread.CurrentPrincipal;
    var user2 = ClaimsPrincipal.Current;

    return "done";
});

app.UseEndpoints(endpoints =>
{
    app.MapDefaultControllerRoute();
    // This method can be used to enable session (or read-only session) on all controllers
    //.RequireSystemWebAdapterSession();

    app.MapReverseProxy();
});

app.Run();

class MyApp : System.Web.HttpApplication
{
    private readonly ILogger<MyApp> _logger;

    public MyApp(ILogger<MyApp> logger)
    {
        _logger = logger;
    }

    protected void Application_Start(int i)
    {
    }

    protected void Application_Init()
    {
        _logger.LogWarning("Application_Init");
    }

    protected void Application_Start()
    {
        _logger.LogWarning("Application_Start");
    }

    protected void Application_BeginRequest()
    {
    }
}

class MyModule : System.Web.IHttpModule
{
    private readonly ILogger<MyModule> _logger;

    public MyModule(ILogger<MyModule> logger)
    {
        _logger = logger;
    }

    public void Dispose()
    {
        _logger.LogWarning("[Dispose] MyModule");
    }

    public void Init(System.Web.HttpApplication application)
    {
        _logger.LogWarning("[Init] MyModule");

        application.BeginRequest += (s, e) =>
        {
            var context = ((System.Web.HttpApplication)s!).Context;
        };

        application.MapRequestHandler += (s, e) =>
        {
            var context = ((System.Web.HttpApplication)s!).Context!;
        };
    }
}
