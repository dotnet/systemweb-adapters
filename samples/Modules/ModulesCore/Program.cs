using System.Web;
using Microsoft.AspNetCore.OutputCaching;
using ModulesLibrary;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSystemWebAdapters()
    .AddHttpApplication<MyApp>(options =>
    {
        // Size of pool for HttpApplication instances. Should be what the expected concurrent requests will be
        options.PoolSize = 10;

        // Register a module by name
        options.RegisterModule<EventsModule>("Events");

        options.NativeModules.Add("NativeModuleSample.dll");
    });

builder.Services.AddOutputCache(options =>
{
    options.AddHttpApplicationBasePolicy(_ => new[] { "browser" });
});

var app = builder.Build();

app.UseSystemWebAdapters();
app.UseOutputCache();

app.MapGet("/", () => "Hello")
    .CacheOutput();

app.Run();

class MyApp : HttpApplication
{
    protected void Application_Start()
    {
    }

    public override string? GetVaryByCustomString(System.Web.HttpContext context, string custom)
    {
        if (custom == "test")
        {
            return "blah";
        }

        return base.GetVaryByCustomString(context, custom);
    }
}
