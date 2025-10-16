using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Features;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddSystemWebAdapters();

var app = builder.Build();

app.UseSystemWebAdapters();

app.Map("/", () => new
{
    Setting1 = AppConfiguration.GetSetting("Setting1"),
    Setting2 = AppConfiguration.GetSetting("Setting2"),
    ConnStr1 = AppConfiguration.GetConnectionString("ConnStr1"),
    ConnStr2 = AppConfiguration.GetConnectionString("ConnStr2")
});

app.Run();
