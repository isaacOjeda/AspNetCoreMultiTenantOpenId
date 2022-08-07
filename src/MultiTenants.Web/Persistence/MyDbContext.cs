using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using MultiTenants.Web.Domain.Entities;

namespace MultiTenants.Web.Persistence;
public class MyDbContext : DbContext
{
    private readonly IMultiTenantContextAccessor<MultiTenantInfo> _tenantAccessor;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    public MyDbContext(
        DbContextOptions<MyDbContext> options,
        IMultiTenantContextAccessor<MultiTenantInfo> tenantAccessor,
        IWebHostEnvironment env,
        IConfiguration config) : base(options)
    {
        _tenantAccessor = tenantAccessor;
        _env = env;
        _config = config;
    }


    public DbSet<Product> Products => Set<Product>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        var connectionString = string.Empty;
        if (_tenantAccessor.MultiTenantContext is null && _env.IsDevelopment())
        {
            connectionString = _config.GetConnectionString("Default");
        }
        else
        {
            connectionString = _tenantAccessor.MultiTenantContext.TenantInfo.ConnectionString;
        }

        optionsBuilder.UseSqlServer(connectionString);
    }
}
