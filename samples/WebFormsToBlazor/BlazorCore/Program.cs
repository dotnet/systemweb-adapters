using BlazorCore;
using BlazorCore.Data;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSystemWebAdapters();
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(options =>
{
    options.RootComponents.RegisterCustomElement<HelloWorld>("hello-world");
});
builder.Services.AddSingleton<WeatherForecastService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapBlazorPages("/_Host");
app.MapReverseProxy();

app.Run();
