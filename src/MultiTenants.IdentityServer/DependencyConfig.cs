﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiTenants.IdentityServer.Domain.Entities.TenantAdmin;
using MultiTenants.IdentityServer.Persistence;

namespace MultiTenants.IdentityServer;
public static class DependencyConfig
{
    public static IServiceCollection AddAspNetServices(this IServiceCollection services)
    {

        services.AddRazorPages();
        services.AddControllers();
        services.AddDatabaseDeveloperPageExceptionFilter();

        return services;
    }

    public static IServiceCollection AddMultiTenantSupport(this IServiceCollection services)
    {
        services.AddMultiTenant<MultiTenantInfo>()
            .WithHostStrategy()
            .WithEFCoreStore<TenantAdminDbContext, MultiTenantInfo>()
            .WithPerTenantAuthentication()
            .WithPerTenantOptions<CookieAuthenticationOptions>((o, tenant) =>
            {
                o.Cookie.Name = $".Identity_{tenant.Identifier}";
                o.LoginPath = "/Identity/Account/Login";
            });

        return services;
    }

    public static IServiceCollection AddIdentity(this IServiceCollection services)
    {
        services.AddDefaultIdentity<IdentityUser>(options =>
        {
            // Password settings.
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;

            // User settings.
            options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;

        }).AddEntityFrameworkStores<IdentityServerDbContext>();

        return services;
    }

    public static IServiceCollection AddDbContexts(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<TenantAdminDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("TenantAdmin")));
        services.AddDbContext<IdentityServerDbContext>();

        return services;
    }

    public static IServiceCollection AddOpenIdConnect(this IServiceCollection services)
    {
        services.AddOpenIddict()

            // Register the OpenIddict core components.
            .AddCore(options =>
            {
                // Configure OpenIddict to use the EF Core stores/models.
                options
                    .UseEntityFrameworkCore()
                    .UseDbContext<IdentityServerDbContext>();
            })
            // Register the OpenIddict server components.
            .AddServer(options =>
            {
                options
                    .AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange()
                    .AllowRefreshTokenFlow();

                options
                    .SetTokenEndpointUris("/connect/token")
                    .SetAuthorizationEndpointUris("/connect/authorize")
                    .SetUserinfoEndpointUris("/connect/userinfo")
                    .SetLogoutEndpointUris("/connect/logout");

                // Encryption and signing of tokens
                options
                    .AddEphemeralEncryptionKey()
                    .AddEphemeralSigningKey()
                    .DisableAccessTokenEncryption();

                // Register scopes (permissions)
                options.RegisterScopes("api");
                options.RegisterScopes("profile");

                // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                options
                    .UseAspNetCore()
                    .EnableTokenEndpointPassthrough()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableUserinfoEndpointPassthrough()
                    .EnableLogoutEndpointPassthrough();
            });

        return services;
    }
}
