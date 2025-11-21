using MachineKeyExample;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection()
    .SetApplicationName(MachineKeyExampleHandler.AppName)
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Path.GetTempPath(), "sharedkeys", MachineKeyExampleHandler.AppName)));

builder.Services.AddSystemWebAdapters();

var app = builder.Build();

app.UseSystemWebAdapters();

app.MapHttpHandler<MachineKeyExampleHandler>("/");

app.Run();
