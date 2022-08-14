using Finbuckle.MultiTenant;
using MultiTenants.IdentityServer;
using MultiTenants.IdentityServer.Domain.Entities.TenantAdmin;
using MultiTenants.IdentityServer.Persistence;
using OpenIddict.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDbContexts(builder.Configuration)
    .AddIdentity()
    .AddOpenIdConnect()
    .AddMultiTenantSupport()
    .AddAspNetServices();


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
            PostLogoutRedirectUris = { new Uri("https://localhost:7221/signout-callback-oidc") },
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,

                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.Endpoints.Logout,


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
            PostLogoutRedirectUris = { new Uri("https://tenant2.localhost:7221/signout-callback-oidc") },
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,

                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.Endpoints.Logout,

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
