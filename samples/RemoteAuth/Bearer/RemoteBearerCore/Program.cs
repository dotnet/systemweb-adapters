using Microsoft.AspNetCore.SystemWebAdapters;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddControllers();

builder.Services.AddSystemWebAdapters()
    .AddRemoteAppClient(remote => remote
        .Configure(options =>
        {
            options.RemoteAppUrl = new(builder.Configuration["ReverseProxy:Clusters:fallbackCluster:Destinations:fallbackApp:Address"]);

            // A real application would not hard code this, but load it
            // securely from environment or configuration
            // Do not re-use this ApiKey; every solution should use a unique ApiKey
            options.ApiKey = "40c807bd-6c00-4e5a-9650-ea20c2e6c02d";
        })

        // This registers the remote app authentication handler. The boolean argument indicates whether remote app auth
        // should be the default scheme. If it is set to false, HTTP requests to authenticate will only be made for
        // endpoints that actually need that behavior, but it is then necessary to annotate endpoints requiring remote app
        // auth with [Authorize(AuthenticationSchemes = RemoteAppAuthenticationDefaults.AuthenticationScheme)] or something similar.
        .AddAuthentication(isDefaultScheme: true));

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapReverseProxy();

app.Run();
