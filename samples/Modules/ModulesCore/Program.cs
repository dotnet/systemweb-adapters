using System.Web;
using ModulesLibrary;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSystemWebAdapters()
    .AddHttpApplication(options =>
    {
        // Size of pool for HttpApplication instances. Should be what the expected concurrent requests will be
        options.PoolSize = 10;

        // Set the HttpApplication type. By default will be HttpApplication, but anything that derives from it is allowed
        options.ApplicationType = typeof(HttpApplication);

        // Register a module by name
        options.RegisterModule<EventsModule>("Events");
    });

var app = builder.Build();

app.UseSystemWebAdapters();

app.Run();

class MyApp : HttpApplication
{
}
