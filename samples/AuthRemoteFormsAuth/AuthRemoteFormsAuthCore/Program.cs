var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddReverseProxy();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add System.Web adapter services, including registering remote app authentication
builder.Services.AddSystemWebAdapters()
    .AddRemoteAppClient(options =>
    {
        options.RemoteAppUrl = new(builder.Configuration["RemoteApp:Url"]!);
        options.ApiKey = builder.Configuration["RemoteApp:ApiKey"]!;
    })
    .AddAuthenticationClient(true);

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Configure the reverse proxy to forward all unhandled requests to the remote app
app.MapForwarder("/{**catch-all}", app.Configuration["RemoteApp:Url"]!)

    // If there is a route locally, we want to ensure that is used by default, but otherwise we'll forward
    .WithOrder(int.MaxValue)

    // If we're going to forward the request, there is no need to run any of the middleware after routing
    .ShortCircuit();

app.Run();
