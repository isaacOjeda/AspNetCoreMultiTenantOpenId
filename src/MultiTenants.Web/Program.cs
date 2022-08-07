using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MultiTenants.Web;
using MultiTenants.Web.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDbContext<TenantAdminDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("TenantAdmin")));
builder.Services.AddDbContext<MyDbContext>();

builder.Services.AddMultiTenant<MultiTenantInfo>()
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


builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
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


await SetupStore(app.Services);

app.Run();

static async Task SetupStore(IServiceProvider sp)
{
    var scopeServices = sp.CreateScope().ServiceProvider;
    var store = scopeServices.GetRequiredService<IMultiTenantStore<MultiTenantInfo>>();

    var tenants = await store.GetAllAsync();

    if (tenants.Count() > 0)
    {
        return;
    }

    await store.TryAddAsync(new MultiTenantInfo
    {
        Id = "tenant-1",
        Identifier = "localhost",
        Name = "My Dev Tenant",
        ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=AspNetCoreMultiTenantOpenId_DevTenant;Trusted_Connection=True;MultipleActiveResultSets=true",
        OpenIdAuthority = "tenant01",
        OpenIdClientId = "tenant01-web-app-secret",
        OpenIdClientSecret = "https://localhost:7193"
    });

    await store.TryAddAsync(new MultiTenantInfo
    {
        Id = "tenant-2",
        Identifier = "tenant2.localhost",
        Name = "My Dev Tenant 2",
        ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=AspNetCoreMultiTenantOpenId_DevTenant02;Trusted_Connection=True;MultipleActiveResultSets=true",
        OpenIdAuthority = "tenant02",
        OpenIdClientId = "tenant02-web-app-secret",
        OpenIdClientSecret = "https://localhost:7193"
    });
}
