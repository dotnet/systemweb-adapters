using System.Security.Claims;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder();
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// These must match the data protection settings in MvcApp Startup.Auth.cs for cookie sharing to work
var sharedApplicationName = "CommonMvcAppName";
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Path.GetTempPath(), "sharedkeys", sharedApplicationName)))
    .SetApplicationName(sharedApplicationName);

builder.Services.AddAuthentication()
    .AddCookie("SharedCookie", options => options.Cookie.Name = ".AspNet.ApplicationCookie");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSystemWebAdapters()
    .AddRemoteAppClient(remote => remote
        .Configure(options =>
        {
            options.RemoteAppUrl = new(builder.Configuration["ReverseProxy:Clusters:fallbackCluster:Destinations:fallbackApp:Address"]);
            options.ApiKey = ClassLibrary.RemoteServiceUtils.ApiKey;
        })
        .AddAuthentication(true)
        .AddSession())
    .AddJsonSessionSerializer(options => ClassLibrary.RemoteServiceUtils.RegisterSessionKeys(options.KnownKeys));

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

app.UseHealthChecks("/healthz", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions() { ResponseWriter = WriteResponse });

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSystemWebAdapters();

app.MapGet("/current-principals-with-metadata", (HttpContext ctx) =>
{
    var user1 = Thread.CurrentPrincipal;
    var user2 = ClaimsPrincipal.Current;

    return "done";
}).WithMetadata(new SetThreadCurrentPrincipalAttribute(), new SingleThreadedRequestAttribute());


app.MapGet("/current-principals-no-metadata", (HttpContext ctx) =>
{
    var user1 = Thread.CurrentPrincipal;
    var user2 = ClaimsPrincipal.Current;

    return "done";
});

app.UseEndpoints(endpoints =>
{
    app.MapDefaultControllerRoute();
    // This method can be used to enable session (or read-only session) on all controllers
    //.RequireSystemWebAdapterSession();

    app.MapReverseProxy();
});

app.Run();

static Task WriteResponse(HttpContext context, HealthReport healthReport)
{
    context.Response.ContentType = "application/json; charset=utf-8";

    var options = new JsonWriterOptions { Indented = true };

    using var memoryStream = new MemoryStream();
    using (var jsonWriter = new Utf8JsonWriter(memoryStream, options))
    {
        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("status", healthReport.Status.ToString());
        jsonWriter.WriteStartObject("results");

        foreach (var healthReportEntry in healthReport.Entries)
        {
            jsonWriter.WriteStartObject(healthReportEntry.Key);
            jsonWriter.WriteString("status",
                healthReportEntry.Value.Status.ToString());
            jsonWriter.WriteString("description",
                healthReportEntry.Value.Description);
            jsonWriter.WriteStartObject("data");

            foreach (var item in healthReportEntry.Value.Data)
            {
                jsonWriter.WritePropertyName(item.Key);

                JsonSerializer.Serialize(jsonWriter, item.Value,
                    item.Value?.GetType() ?? typeof(object));
            }

            jsonWriter.WriteEndObject();
            jsonWriter.WriteEndObject();
        }

        jsonWriter.WriteEndObject();
        jsonWriter.WriteEndObject();
    }

    return context.Response.WriteAsync(
        Encoding.UTF8.GetString(memoryStream.ToArray()));
}
