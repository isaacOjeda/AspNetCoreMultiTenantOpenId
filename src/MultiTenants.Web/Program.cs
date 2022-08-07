using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using MultiTenants.Web;
using MultiTenants.Web.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddMultiTenant<MultiTenantInfo>()
    .WithHostStrategy()
    .WithEFCoreStore<TenantAdminDbContext, MultiTenantInfo>();
builder.Services.AddDbContext<TenantAdminDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("TenantAdmin")));
builder.Services.AddDbContext<MyDbContext>();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
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


await SetupStore(app.Services);

app.Run();

async Task SetupStore(IServiceProvider sp)
{
    var scopeServices = sp.CreateScope().ServiceProvider;
    var store = scopeServices.GetRequiredService<IMultiTenantStore<MultiTenantInfo>>();

    await store.TryAddAsync(new MultiTenantInfo
    {
        Id = "tenant-1",
        Identifier = "localhost",
        Name = "My Dev Tenant",
        ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=AspNetCoreMultiTenantOpenId_DevTenant;Trusted_Connection=True;MultipleActiveResultSets=true"
    });

    await store.TryAddAsync(new MultiTenantInfo
    {
        Id = "tenant-2",
        Identifier = "tenant2.localhost",
        Name = "My Dev Tenant 2",
        ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=AspNetCoreMultiTenantOpenId_DevTenant02;Trusted_Connection=True;MultipleActiveResultSets=true"
    });
}
