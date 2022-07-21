# ASP.NET Core Configuration

In order to setup the adapters for usage within an ASP.NET Core app, the services must be registered and middleware must be inserted. The modified `Program.cs` looks like this:

```cs
using Microsoft.AspNetCore.SystemWebAdapters;

var builder = WebApplication.CreateBuilder();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSystemWebAdapters()
    .AddRemoteApp(options =>
    {
        options.RemoteApp = new(builder.Configuration["ReverseProxy:Clusters:fallbackCluster:Destinations:fallbackApp:Address"]);
        options.ApiKey = ClassLibrary.SessionUtils.ApiKey;
    })
    .AddRemoteAppSession()
    .AddJsonSessionSerializer(options => ClassLibrary.SessionUtils.RegisterSessionKeys(options));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSystemWebAdapters();
app.MapDefaultControllerRoute()
    .BufferResponseStream()
    .PreBufferRequestStream()
    .RequireSystemWebAdapterSession();

app.Run();
```

This opts into all the behavior (described below), as well as sets up a remote session state (see [here](session-state/session.md) for details).

