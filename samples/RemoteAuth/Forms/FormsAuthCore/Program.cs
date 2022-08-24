var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add System.Web adapter services, including registering remote app authentication
builder.Services.AddSystemWebAdapters()
    .AddRemoteAppClient(remote => remote
        .Configure(options =>
        {
            options.RemoteAppUrl = new(builder.Configuration["ReverseProxy:Clusters:fallbackCluster:Destinations:fallbackApp:Address"]);

            // Do not re-use this ApiKey; every solution should use a unique ApiKey
            options.ApiKey = "8e470586-24e5-4f2a-8245-69bbdbf9f767";
        })
        .AddAuthentication(true));

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
app.MapReverseProxy();

app.Run();
