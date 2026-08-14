using BlazorWebApp.Components;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddSystemWebAdapters()
    .AddJsonSessionSerializer(options =>
    {
        options.RegisterKey<string>("test-value");
    });

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options => options.RootComponents.RegisterCustomElement<HelloWorld>("hello-world"));

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.Use(async (ctx, next) =>
{
    var i = ctx.Request.Path.StartsWithSegments("/_blazor", StringComparison.OrdinalIgnoreCase);
    await next(ctx);

    if(ctx.Response.StatusCode >= 300)
    {
        var path = ctx.Request.Path;
        var method = ctx.Request.Method;
    }
});
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.UseWhen(
    (context) => !HttpMethods.IsConnect(context.Request.Method),
    appBuilder => appBuilder.UseSystemWebAdapters());

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .RequireSystemWebAdapterSession();

app.MapRemoteAppFallback()
    .ShortCircuit();

app.Run();
