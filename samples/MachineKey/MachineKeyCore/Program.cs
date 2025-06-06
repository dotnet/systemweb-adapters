using MachineKeyExample;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection()
    .SetApplicationName(MachineKeyExtensions.AppName)
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Path.GetTempPath(), "sharedkeys", MachineKeyExtensions.AppName)));

builder.Services.AddSystemWebAdapters();

var app = builder.Build();

app.UseSystemWebAdapters();

app.Map("/", (HttpContext context) =>
{
    context.Features.GetRequiredFeature<IHttpBodyControlFeature>().AllowSynchronousIO = true;
    context.AsSystemWeb().ProcessMachineKeyRequest();
});

app.Run();
