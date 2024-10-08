var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSystemWebAdapters()
    .AddRemoteAppClient(options =>
    {
        options.RemoteAppUrl = new(builder.Configuration["ProxyTo"]!);
        options.ApiKey = builder.Configuration["RemoteAppApiKey"]!;
    })
    .AddAuthenticationClient(true);

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSystemWebAdapters();

app.MapDefaultControllerRoute();
app.MapForwarder("/{**catch-all}", app.Configuration["ProxyTo"]!).WithOrder(int.MaxValue);

app.Run();
