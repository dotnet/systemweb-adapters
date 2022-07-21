using System.Security.Claims;
using Microsoft.AspNetCore.SystemWebAdapters;

var builder = WebApplication.CreateBuilder();
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSystemWebAdapters()
    .AddRemoteApp(options =>
    {
        options.RemoteAppUrl = new(builder.Configuration["ReverseProxy:Clusters:fallbackCluster:Destinations:fallbackApp:Address"]);
        options.ApiKey = ClassLibrary.RemoteServiceUtils.ApiKey;
    })
    .AddRemoteAppSession()
    .AddJsonSessionSerializer(options => ClassLibrary.RemoteServiceUtils.RegisterSessionKeys(options.KnownKeys))
    .AddRemoteAppClientAuthentication(true);

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

app.MapGet("/current-principals-with-metadata", (HttpContext ctx) =>
{
    var user1 = Thread.CurrentPrincipal;
    var user2 = ClaimsPrincipal.Current;

    return "done";
}).WithMetadata(new SetThreadCurrentPrincipalAttribute(), new SingleThreadedRequestAttribute());


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
