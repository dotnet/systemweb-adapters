using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Authentication;

var builder = WebApplication.CreateBuilder();
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSystemWebAdapters()
    .AddJsonSessionSerializer(options => ClassLibrary.RemoteServiceUtils.RegisterSessionKeys(options))
    .AddRemoteAppSession(ConfigureRemoteServiceOptions)
    .AddRemoteAuthentication(o => ConfigureRemoteServiceOptions(o.RemoteServiceOptions));

void ConfigureRemoteServiceOptions(RemoteServiceOptions options)
{
    options.RemoteAppUrl = new(builder.Configuration["ReverseProxy:Clusters:fallbackCluster:Destinations:fallbackApp:Address"]);
    options.ApiKey = ClassLibrary.RemoteServiceUtils.ApiKey;
}

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
    app.MapDefaultControllerRoute()
       // This enables remote authentication for all endpoints.
       // Use the [RemoteAuthentication] attribute on specific
       // controllers or actions, instead, to only use remote
       // authentication with a subset of endpoints.
       .RequireRemoteAuthentication();
    // This method can be used to enable session (or read-only session) on all controllers
    //.RequireSystemWebAdapterSession();

    app.MapReverseProxy();
});

app.Run();
