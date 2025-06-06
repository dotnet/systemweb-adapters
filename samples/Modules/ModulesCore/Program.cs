using System.Web;
using Microsoft.AspNetCore.OutputCaching;
using ModulesLibrary;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSystemWebAdapters()
    .AddHttpApplication(options =>
    {
        // Size of pool for HttpApplication instances. Should be what the expected concurrent requests will be
        options.PoolSize = 10;

        // These have a bit of a cost to them, so only enable if you need them
        options.ArePreSendEventsEnabled = true;

        // Register a module by name
        options.RegisterModule<EventsModule>("Events");
    });

builder.Services.AddOutputCache(options =>
{
    options.AddHttpApplicationBasePolicy(_ => new[] { "browser" });
});

var app = builder.Build();

app.UseSystemWebAdapters();

app.MapGet("/", () => "Hello World!\n");
app.Run();
