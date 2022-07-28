using Microsoft.EntityFrameworkCore;
using MultiTenants.Web.Domain.Entities.TenantAdmin;

namespace MultiTenants.Web.Persistence;
public class TenantAdminDbContext : DbContext
{
    public TenantAdminDbContext(DbContextOptions<TenantAdminDbContext> options)
        : base(options)
    {

    }


    public DbSet<Tenant> Tenants => Set<Tenant>();
}
