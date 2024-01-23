using BlazorCore;
using BlazorCore.Components;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

// Add YARP
builder.Services.AddHttpForwarder();

// Add constraint to redirect .axd files to WebForms
builder.Services.AddRouting(options => options.ConstraintMap.Add("isAxdFile", typeof(AxdContraint)));
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
app.UseSystemWebAdapters();

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
