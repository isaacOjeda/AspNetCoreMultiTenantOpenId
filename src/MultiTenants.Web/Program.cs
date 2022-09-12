using Finbuckle.MultiTenant;
using MultiTenants.Web;
using MultiTenants.Web.Domain.Entities.TenantAdmin;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddRazorPages();
builder.Services.AddDbContexts(builder.Configuration.GetConnectionString("TenantAdmin"));
builder.Services.AddMuiltiTenantSupport();
builder.Services.AddAuthentications();


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
        OpenIdClientId = "tenant01",
        OpenIdClientSecret = "tenant01-web-app-secret",
        OpenIdAuthority = "https://localhost:7193"
    });

    await store.TryAddAsync(new MultiTenantInfo
    {
        Id = "tenant-2",
        Identifier = "tenant2.localhost",
        Name = "My Dev Tenant 2",
        ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=AspNetCoreMultiTenantOpenId_DevTenant02;Trusted_Connection=True;MultipleActiveResultSets=true",
        OpenIdClientId = "tenant02",
        OpenIdClientSecret = "tenant02-web-app-secret",
        OpenIdAuthority = "https://tenant2.localhost:7193"
    });
}
