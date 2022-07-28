using Microsoft.EntityFrameworkCore;
using MultiTenants.Fx;
using MultiTenants.Fx.Contracts;
using MultiTenants.Web.Domain.Entities;

namespace MultiTenants.Web.Persistence;
public class MyDbContext : DbContext
{
    private readonly Tenant _tenant;

    public MyDbContext(
        DbContextOptions<MyDbContext> options,
        ITenantAccessor<Tenant> tenantAccessor) : base(options)
    {
        _tenant = tenantAccessor.Tenant ?? throw new ArgumentException(nameof(Tenant));
    }


    public DbSet<Product> Products => Set<Product>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseSqlServer(_tenant.Items["ConnectionString"].ToString()!);
    }
}
