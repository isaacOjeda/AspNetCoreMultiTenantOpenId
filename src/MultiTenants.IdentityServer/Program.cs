using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiTenants.IdentityServer;
using MultiTenants.IdentityServer.Domain.Entities.TenantAdmin;
using MultiTenants.IdentityServer.Persistence;
using OpenIddict.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TenantAdminDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("TenantAdmin")));
builder.Services.AddDbContext<IdentityServerDbContext>();
builder.Services.AddDefaultIdentity<IdentityUser>()
    .AddEntityFrameworkStores<IdentityServerDbContext>();

builder.Services.AddOpenIddict()

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

builder.Services.AddMultiTenant<MultiTenantInfo>()
    .WithBasePathStrategy()
    .WithEFCoreStore<TenantAdminDbContext, MultiTenantInfo>();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.Add(new MultiTenantRouteConvention());
});
builder.Services.AddControllers();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseMultiTenant();
app.Use(async (context, next) =>
{
    using var scope = context.RequestServices.CreateScope();

    var tenantContext = scope.ServiceProvider.GetRequiredService<IMultiTenantContextAccessor<MultiTenantInfo>>();

    if (tenantContext.MultiTenantContext?.TenantInfo is null)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync("Missing tenant");

        return;
    }

    await next(context);
});
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

await SeedDefaultClients();
await SetupTenants();

app.Run();



// OpenIddict info
async Task SeedDefaultClients()
{
    using var scope = app.Services.CreateScope();

    var context = scope.ServiceProvider.GetRequiredService<IdentityServerDbContext>();
    var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

    await context.Database.EnsureCreatedAsync();

    var client = await manager.FindByClientIdAsync("tenant01");

    if (client is null)
    {
        await manager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "tenant01",
            ClientSecret = "tenant01-web-app-secret",
            DisplayName = "Multi-Tenant Web Application 01",
            RedirectUris = { new Uri("https://localhost:7221/signin-oidc") },
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,

                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,


                OpenIddictConstants.Permissions.Prefixes.Scope + "api",
                OpenIddictConstants.Permissions.Prefixes.Scope + "profile",
                OpenIddictConstants.Permissions.ResponseTypes.Code
            }
        });

        await manager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "tenant02",
            ClientSecret = "tenant02-web-app-secret",
            DisplayName = "Multi-Tenant Web Application 02",
            RedirectUris = { new Uri("https://tenant2.localhost:7221/signin-oidc") },
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,

                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,


                OpenIddictConstants.Permissions.Prefixes.Scope + "api",
                OpenIddictConstants.Permissions.Prefixes.Scope + "profile",
                OpenIddictConstants.Permissions.ResponseTypes.Code
            }
        });
    }
}
// Multitenant info
async Task SetupTenants()
{
    using var scope = app.Services.CreateScope();

    var store = scope.ServiceProvider.GetRequiredService<IMultiTenantStore<MultiTenantInfo>>();

    var tenants = await store.GetAllAsync();

    if (tenants.Count() > 0)
    {
        return;
    }

    await store.TryAddAsync(new MultiTenantInfo
    {
        Id = Guid.NewGuid().ToString(),
        Identifier = "tenant1",
        Name = "My Identity Dev Tenant",
        ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=AspNetCoreMultiTenantOpenId_IdentityServer01;Trusted_Connection=True;MultipleActiveResultSets=true"
    });

    await store.TryAddAsync(new MultiTenantInfo
    {
        Id = Guid.NewGuid().ToString(),
        Identifier = "tenant2",
        Name = "My Identity Dev Tenant 2",
        ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=AspNetCoreMultiTenantOpenId_IdentityServer02;Trusted_Connection=True;MultipleActiveResultSets=true"
    });
}
