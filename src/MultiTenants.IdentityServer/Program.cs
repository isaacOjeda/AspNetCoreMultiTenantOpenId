using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiTenants.IdentityServer.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<IdentityServerDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    // options.UseOpenIddict();
});

// builder.Services.AddOpenIddict()

//     // Register the OpenIddict core components.
//     .AddCore(options =>
//     {
//         // Configure OpenIddict to use the EF Core stores/models.
//         options
//             .UseEntityFrameworkCore()
//             .UseDbContext<ApplicationDbContext>();
//     })
//     // Register the OpenIddict server components.
//     .AddServer(options =>
//     {
//         options
//             .AllowClientCredentialsFlow()
//             .AllowAuthorizationCodeFlow()
//             .RequireProofKeyForCodeExchange()
//             .AllowRefreshTokenFlow();

//         options
//             .SetTokenEndpointUris("/connect/token")
//             .SetAuthorizationEndpointUris("/connect/authorize")
//             .SetUserinfoEndpointUris("/connect/userinfo");

//         // Encryption and signing of tokens
//         options
//             .AddEphemeralEncryptionKey()
//             .AddEphemeralSigningKey()
//             .DisableAccessTokenEncryption();

//         // Register scopes (permissions)
//         options.RegisterScopes("api");
//         options.RegisterScopes("profile");

//         // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
//         options
//             .UseAspNetCore()
//             .EnableTokenEndpointPassthrough()
//             .EnableAuthorizationEndpointPassthrough()
//             .EnableUserinfoEndpointPassthrough();
//     });


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

// await SeedDefaultClients();

app.Run();

