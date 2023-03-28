using System.Web;
using ModulesLibrary;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSystemWebAdapters()
    .AddHttpApplication<MyApp>(options =>
    {
        // Size of pool for HttpApplication instances. Should be what the expected concurrent requests will be
        options.PoolSize = 10;

        // Register a module by name
        options.RegisterModule<EventsModule>("Events");
    });

var app = builder.Build();

app.UseSystemWebAdapters();

app.Run();

class MyApp : HttpApplication
{
    protected void Application_Start()
    {
    }
}
