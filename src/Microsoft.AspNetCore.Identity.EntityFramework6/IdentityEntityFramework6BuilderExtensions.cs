// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFramework6;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Contains extension methods to <see cref="IdentityBuilder"/> for adding Entity Framework 6 stores.
/// </summary>
public static class IdentityEntityFramework6BuilderExtensions
{
    public static IdentityCookiesBuilder AddIdentityCookies(this AuthenticationBuilder builder, string schemeName, string cookieName = ".AspNet.ApplicationCookie")
    {
        var cookieBuilder = new IdentityCookiesBuilder();
        cookieBuilder.ApplicationCookie = builder.AddApplicationCookie(schemeName, cookieName);
        cookieBuilder.ExternalCookie = builder.AddExternalCookie();
        cookieBuilder.TwoFactorRememberMeCookie = builder.AddTwoFactorRememberMeCookie();
        cookieBuilder.TwoFactorUserIdCookie = builder.AddTwoFactorUserIdCookie();
        return cookieBuilder;
    }

    /// <summary>
    /// Adds the identity application cookie.
    /// </summary>
    /// <param name="builder">The current <see cref="AuthenticationBuilder"/> instance.</param>
    /// <returns>The <see cref="OptionsBuilder{TOptions}"/> which can be used to configure the cookie authentication.</returns>
    public static OptionsBuilder<CookieAuthenticationOptions> AddApplicationCookie(this AuthenticationBuilder builder, string schemeName, string cookieName)
    {
        builder.AddCookie(schemeName, o =>
        {
            o.Cookie.Name = cookieName;
            o.LoginPath = new PathString("/Account/Login");
            o.Events = new CookieAuthenticationEvents
            {
                OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync
            };
        });
        return new OptionsBuilder<CookieAuthenticationOptions>(builder.Services, schemeName);
    }

    public static void AddEntityFramework6DbContext<TContext>(this IServiceCollection services, String connectionString)
        where TContext : System.Data.Entity.DbContext
    {
        services.TryAddScoped<TContext>(provider =>
        {
            var context = ActivatorUtilities.CreateInstance<TContext>(provider, connectionString);
            return context;
        });
    }

    /// <summary>
    /// Adds an Entity Framework 6 implementation of identity information stores.
    /// </summary>
    /// <typeparam name="TContext">The Entity Framework 6 database context to use.</typeparam>
    /// <param name="builder">The <see cref="IdentityBuilder"/> instance this method extends.</param>
    /// <returns>The <see cref="IdentityBuilder"/> instance this method extends.</returns>
    public static IdentityBuilder AddEntityFramework6Stores<TContext>(this IdentityBuilder builder)
        where TContext : System.Data.Entity.DbContext
    {
        ArgumentNullException.ThrowIfNull(builder);

        AddStores(builder.Services, builder.UserType, builder.RoleType, typeof(TContext));

        return builder;
    }

    private static void AddStores(IServiceCollection services, Type userType, Type? roleType, Type contextType)
    {
        var identityUserType = FindGenericBaseType(userType, typeof(IdentityUser<>));
        if (identityUserType == null)
        {
            throw new InvalidOperationException($"Type {userType.Name} does not derive from IdentityUser<TKey>.");
        }

        var keyType = identityUserType.GenericTypeArguments[0];

        if (roleType != null)
        {
            var identityRoleType = FindGenericBaseType(roleType, typeof(IdentityRole<>));
            if (identityRoleType == null)
            {
                throw new InvalidOperationException($"Type {roleType.Name} does not derive from IdentityRole<TKey>.");
            }

            Type userStoreType;
            Type roleStoreType;
            var identityContext = FindGenericBaseType(contextType, typeof(IdentityEF6DbContext<,,,,,>));
            if (identityContext == null)
            {
                // If it's a custom DbContext, we can only add the default POCOs
                userStoreType = typeof(UserStoreEF6<,,,>).MakeGenericType(userType, roleType, contextType, keyType);
                roleStoreType = typeof(RoleStoreEF6<,,>).MakeGenericType(roleType, contextType, keyType);
            }
            else
            {
                // Use the full generic store with all the entity types from the context
                userStoreType = typeof(UserStoreEF6<,,,,,,,,>).MakeGenericType(
                    userType,
                    roleType,
                    contextType,
                    identityContext.GenericTypeArguments[2],
                    identityContext.GenericTypeArguments[5], // TUserClaim
                    identityContext.GenericTypeArguments[4], // TUserRole
                    identityContext.GenericTypeArguments[3], // TUserLogin
                    typeof(IdentityUserTokenEF6<>).MakeGenericType(identityContext.GenericTypeArguments[2]), // TUserToken
                    typeof(IdentityRoleClaimEF6<>).MakeGenericType(identityContext.GenericTypeArguments[2])); // TRoleClaim

                roleStoreType = typeof(RoleStoreEF6<,,,,>).MakeGenericType(
                    roleType,
                    contextType,
                    identityContext.GenericTypeArguments[2], // TKey
                    identityContext.GenericTypeArguments[4], // TUserRole
                    typeof(IdentityRoleClaimEF6<>).MakeGenericType(identityContext.GenericTypeArguments[2])); // TRoleClaim
            }

            services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), userStoreType);
            services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(roleType), roleStoreType);
        }
        else
        {
            Type userStoreType;
            var identityContext = FindGenericBaseType(contextType, typeof(IdentityEF6DbContext<,,,,,>));
            if (identityContext == null)
            {
                userStoreType = typeof(UserOnlyStoreEF6<,,>).MakeGenericType(userType, contextType, keyType);
            }
            else
            {
                userStoreType = typeof(UserOnlyStoreEF6<,,,,,>).MakeGenericType(
                     userType,
                     contextType,
                     identityContext.GenericTypeArguments[2], // TKey
                     identityContext.GenericTypeArguments[5], // TUserClaim
                     identityContext.GenericTypeArguments[3], // TUserLogin
                     typeof(IdentityUserTokenEF6<>).MakeGenericType(identityContext.GenericTypeArguments[2])); // TUserToken
            }
            services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), userStoreType);
        }
    }

    private static Type? FindGenericBaseType(Type currentType, Type genericBaseType)
    {
        var type = currentType;
        while (type != null)
        {
            var genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
            if (genericType != null && genericType == genericBaseType)
            {
                return type;
            }
            type = type.BaseType;
        }
        return null;
    }
}
