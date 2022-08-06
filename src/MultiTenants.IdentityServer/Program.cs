using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiTenants.IdentityServer.Persistence;
using OpenIddict.Abstractions;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<IdentityServerDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.UseOpenIddict();
});

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


builder.Services.AddDefaultIdentity<IdentityUser>()
    .AddEntityFrameworkStores<IdentityServerDbContext>();
builder.Services.AddRazorPages();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

await SeedDefaultClients();

app.Run();



async Task SeedDefaultClients()
{
    using var scope = app.Services.CreateScope();

    var context = scope.ServiceProvider.GetRequiredService<IdentityServerDbContext>();
    var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

    await context.Database.EnsureCreatedAsync();

    var client = await manager.FindByClientIdAsync("multitenantweb");

    if (client is null)
    {
        await manager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "multitenantweb",
            ClientSecret = "multitenantweb-web-app-secret",
            DisplayName = "Multi-Tenant Web Application",
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
    }
}