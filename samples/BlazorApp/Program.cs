using BlazorApp.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddSystemWebAdapters()
    .AddJsonSessionSerializer(options => ClassLibrary.RemoteServiceUtils.RegisterSessionKeys(options.KnownKeys))
    .AddRemoteAppClient(options =>
    {
        options.RemoteAppUrl = new(builder.Configuration["ReverseProxy:Clusters:fallbackCluster:Destinations:fallbackApp:Address"]!);
        options.ApiKey = builder.Configuration["RemoteAppApiKey"]!;
    })
    .AddAuthenticationClient(true)
    .AddSessionClient();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSystemWebAdapters();

app.MapBlazorHub();

// This is the default way to set up Blazor pages
//app.MapFallbackToPage("/_Host");

// Important: Map explicitly so that deep links continue to work when proxying
app.MapBlazorPages("/_Host");

app.MapReverseProxy();

app.Run();
