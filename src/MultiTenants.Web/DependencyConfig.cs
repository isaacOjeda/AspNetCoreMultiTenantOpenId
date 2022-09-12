using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MultiTenants.Web.Domain.Entities.TenantAdmin;
using MultiTenants.Web.Persistence;

namespace MultiTenants.Web;

public static class DependencyConfig
{
    public static IServiceCollection AddAuthentications(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie()
        .AddOpenIdConnect();

        return services;
    }

    public static IServiceCollection AddDbContexts(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<TenantAdminDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddDbContext<MyDbContext>();

        return services;
    }

    public static IServiceCollection AddMuiltiTenantSupport(this IServiceCollection services)
    {
        services.AddMultiTenant<MultiTenantInfo>()
            .WithHostStrategy()
            .WithEFCoreStore<TenantAdminDbContext, MultiTenantInfo>()
            .WithPerTenantAuthentication()
            .WithPerTenantOptions<CookieAuthenticationOptions>((options, tenant) =>
            {
                options.Cookie.Name = $".Auth{tenant.Id}";
            })
            .WithPerTenantOptions<OpenIdConnectOptions>((options, tenant) =>
            {
                options.Scope.Add("openid");
                options.Scope.Add("profile");

                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.TokenValidationParameters.NameClaimType = "name";
                options.ResponseType = OpenIdConnectResponseType.Code;

                options.Authority = tenant.OpenIdAuthority;
                options.ClientId = tenant.OpenIdClientId;
                options.ClientSecret = tenant.OpenIdClientSecret;
            });

        return services;
    }
}