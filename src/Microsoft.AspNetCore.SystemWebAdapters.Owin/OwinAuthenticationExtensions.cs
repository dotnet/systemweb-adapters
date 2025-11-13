// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Owin;

namespace Microsoft.Extensions.DependencyInjection;

public static class OwinAuthenticationExtensions
{
    public static AuthenticationBuilder AddOwinAuthentication(this AuthenticationBuilder builder, Action<IAppBuilder, IServiceProvider> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        return builder.AddOwinAuthentication(OwinAuthenticationDefaults.AuthenticationScheme, configure);
    }

    public static AuthenticationBuilder AddOwinAuthentication(this AuthenticationBuilder builder, string scheme, Action<IAppBuilder, IServiceProvider> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(scheme);
        ArgumentNullException.ThrowIfNull(configure);

        builder.Services.AddSingleton<IPostConfigureOptions<IdentityAuthenticationSchemeOptions>, BuildOwinRequestDelegate>();
        builder.AddScheme<IdentityAuthenticationSchemeOptions, OwinAuthenticationHandler>(scheme, options =>
        {
            options.OwinPipeline = configure;
        });

        return builder;
    }

    private sealed class OwinAuthenticationHandler : AuthenticationHandler<IdentityAuthenticationSchemeOptions>, IAuthenticationSignOutHandler
    {
        public OwinAuthenticationHandler(
            IOptionsMonitor<IdentityAuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        public Task SignOutAsync(AuthenticationProperties? properties)
        {
            Context.GetOwinContext().Authentication.SignOut(CreateOwinProperties(properties));
            return Task.CompletedTask;
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Context.GetOwinContext().Authentication.Challenge(CreateOwinProperties(properties));

            return Task.CompletedTask;
        }

        private static Microsoft.Owin.Security.AuthenticationProperties? CreateOwinProperties(AuthenticationProperties? properties)
        {
            if (properties is null)
            {
                return null;
            }

            return new(properties.Items);
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Options.RequestDelegate is not { } requestDelegate)
            {
                return AuthenticateResult.NoResult();
            }

            await Context.RunForkedPipelineAsync(typeof(OwinAuthenticationHandler), requestDelegate);

            if (Context.User is { Identity.IsAuthenticated: true } user)
            {
                return AuthenticateResult.Success(new AuthenticationTicket(user, Scheme.Name));
            }

            return AuthenticateResult.NoResult();
        }
    }

    private sealed class BuildOwinRequestDelegate(IServiceProvider services) : IPostConfigureOptions<IdentityAuthenticationSchemeOptions>
    {
        public void PostConfigure(string? name, IdentityAuthenticationSchemeOptions options)
        {
            var application = new ApplicationBuilder(services);

            if (options.OwinPipeline is not { })
            {
                throw new InvalidOperationException("No OWIN pipeline configured for OWIN authentication.");
            }

            application.UseOwin(app =>
            {
                app.Properties[OwinConstants.IntegratedPipelineStageMarker] = (IAppBuilder _, string name) =>
                {
                    if (!string.Equals(name, OwinConstants.StageAuthenticate, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException($"Using the OWIN authentication handler only allows the authentication stage. '{name}' was use. Consider using IApplicationBuilder.UseOwin to incorporate OWIN pipeline into the normal middleware.");
                    }
                };

                options.OwinPipeline(app, services);
                app.Run(ctx => ctx.GetHttpContext().JoinPipelineFork(typeof(OwinAuthenticationHandler)));
            });

            options.RequestDelegate = application.Build();
        }
    }

    private sealed class IdentityAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public Func<HttpContext, AuthenticationProperties?, Task>? SignOut { get; set; }

        public Action<IAppBuilder, IServiceProvider>? OwinPipeline { get; set; }

        internal RequestDelegate? RequestDelegate { get; set; }
    }
}

