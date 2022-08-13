using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiTenants.IdentityServer.Domain.Entities.TenantAdmin;
using MultiTenants.IdentityServer.Persistence;

namespace MultiTenants.IdentityServer;
public static class DependencyConfig
{
    public static IServiceCollection AddAspNetServices(this IServiceCollection services)
    {

        services.AddRazorPages(options =>
        {
            options.Conventions.Add(new MultiTenantRouteConvention());
        });
        services.AddControllers();
        services.AddDatabaseDeveloperPageExceptionFilter();

        return services;
    }

    public static IServiceCollection AddMultiTenantSupport(this IServiceCollection services)
    {
        services.AddMultiTenant<MultiTenantInfo>()
            .WithBasePathStrategy()
            .WithEFCoreStore<TenantAdminDbContext, MultiTenantInfo>()
            .WithPerTenantAuthentication()
            .WithPerTenantOptions<CookieAuthenticationOptions>((options, tenant) =>
            {
                options.Cookie.Name = $".IdentityAuth{tenant.Id}";
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

            // Lockout settings.
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

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
                    .AllowClientCredentialsFlow()
                    .AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange()
                    .AllowRefreshTokenFlow();

                options
                    .SetTokenEndpointUris("/connect/token")
                    .SetAuthorizationEndpointUris("/connect/authorize")
                    .SetUserinfoEndpointUris("/connect/userinfo");

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
                    .EnableUserinfoEndpointPassthrough();
            });

        return services;
    }
}
