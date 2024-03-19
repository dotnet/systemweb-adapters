using BlazorCore;
using BlazorCore.Components;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

// Add YARP
builder.Services.AddHttpForwarder();

// Add constraint to redirect .axd files to WebForms
builder.Services.AddRouting(options => options.ConstraintMap.Add("isAxdFile", typeof(AxdConstraint)));
// Add System Web Adapters and setup session
builder.Services.AddSystemWebAdapters()
    .AddJsonSessionSerializer(options =>
    {
        options.RegisterKey<string>("test-value");
    })
    .AddRemoteAppClient(options =>
    {
        options.RemoteAppUrl = new(builder.Configuration["ProxyTo"]);
        options.ApiKey = builder.Configuration["RemoteAppApiKey"];
    })
    .AddSessionClient();

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
 * TLDR: We need to ensure that System.Web.Adapters is not used with Blazor SignalR
 */
app.UseWhen(
    (context) => HttpMethods.IsConnect(context.Request.Method) == false,
    appBuilder => appBuilder.UseSystemWebAdapters());
//app.UseWhen(
//    (context) => context.Request.Path.ToString().Contains("/_blazor", StringComparison.OrdinalIgnoreCase) == false,
//    appBuilder => appBuilder.UseSystemWebAdapters());

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .RequireSystemWebAdapterSession();

app.MapForwarder("/Scripts/{**catch-all}", app.Configuration["ProxyTo"]).Add(static builder => ((RouteEndpointBuilder)builder).Order = int.MaxValue);
app.MapForwarder("/Content/{**catch-all}", app.Configuration["ProxyTo"]).Add(static builder => ((RouteEndpointBuilder)builder).Order = int.MaxValue);
app.MapForwarder("/bundles/{**catch-all}", app.Configuration["ProxyTo"]).Add(static builder => ((RouteEndpointBuilder)builder).Order = int.MaxValue);
app.MapForwarder("/About", app.Configuration["ProxyTo"]).Add(static builder => ((RouteEndpointBuilder)builder).Order = int.MaxValue);
app.MapForwarder("/Contact", app.Configuration["ProxyTo"]).Add(static builder => ((RouteEndpointBuilder)builder).Order = int.MaxValue);
app.MapForwarder("/{route:isAxdFile}", app.Configuration["ProxyTo"]).Add(static builder => ((RouteEndpointBuilder)builder).Order = int.MaxValue);
app.Run();
