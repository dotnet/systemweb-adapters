using MachineKeyExample;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection()
    .SetApplicationName(MachineKeyTest.AppName)
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Path.GetTempPath(), "sharedkeys", MachineKeyTest.AppName)));

builder.Services.AddSystemWebAdapters();

var app = builder.Build();

app.UseSystemWebAdapters();

app.Map("/", (HttpContext context) =>
{
    context.Features.GetRequiredFeature<IHttpBodyControlFeature>().AllowSynchronousIO = true;
    MachineKeyTest.Run(context);
});

app.Run();
