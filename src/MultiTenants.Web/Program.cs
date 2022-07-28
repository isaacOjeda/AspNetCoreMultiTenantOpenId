using Microsoft.EntityFrameworkCore;
using MultiTenants.Fx;
using MultiTenants.Fx.Contracts;
using MultiTenants.Web.MultiTenancy;
using MultiTenants.Web.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddMultiTenancy()
    .WithResolutionStrategy<HostResolutionStrategy>()
    .WithStore<DbContextTenantStore>();

builder.Services.AddDbContext<TenantAdminDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("TenantAdmin")));

builder.Services.AddDbContext<MyDbContext>();
builder.Services.AddTransient<ITenantAccessor<Tenant>, TenantAccessor>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMultiTenancy();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
