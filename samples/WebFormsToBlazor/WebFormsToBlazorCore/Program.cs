using Microsoft.AspNetCore.Components.Web;
using WebFormsToBlazorCore;
using WebFormsToBlazorCore.Components;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddSystemWebAdapters()
    .AddJsonSessionSerializer(options =>
    {
        options.RegisterKey<string>("test-value");
    });

// Add YARP
builder.Services.AddHttpForwarder();

// Add constraint to redirect .axd files to WebForms
builder.Services.AddRouting(options => options.ConstraintMap.Add("isAxdFile", typeof(AxdConstraint)));
// Add System Web Adapters and setup session

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options => options.RootComponents.RegisterCustomElement<HelloWorld>("hello-world"));

var app = builder.Build();

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

/*
 * When Blazor interactive SSR establishes a SignalR connection, a CONNECT method is requested.
 * This request will stay open until Blazor sends a disconnect request. 
 * Since it stays open, the session is not properly committed and will stay in the heartbeat loop on the framework side of System.WebAdapters.
 * We can either:
 *     A: Block all connect requests from using System.Web.Adapters
 *     B: Block all requests from paths containing "/_blazor"
 * TLDR: We need to ensure that SystemWeb.Adapters is not used with Blazor SignalR
 */
app.UseWhen(
    (context) => !HttpMethods.IsConnect(context.Request.Method),
    appBuilder => appBuilder.UseSystemWebAdapters());
//app.UseWhen(
//    (context) => context.Request.Path.ToString().Contains("/_blazor", StringComparison.OrdinalIgnoreCase) == false,
//    appBuilder => appBuilder.UseSystemWebAdapters());

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .RequireSystemWebAdapterSession();

app.MapRemoteAppFallback("/Scripts/{**catch-all}");
app.MapRemoteAppFallback("/Content/{**catch-all}");
app.MapRemoteAppFallback("/bundles/{**catch-all}");
app.MapRemoteAppFallback("/About");
app.MapRemoteAppFallback("/Contact");
app.MapRemoteAppFallback("/{route:isAxdFile}");

app.Run();
