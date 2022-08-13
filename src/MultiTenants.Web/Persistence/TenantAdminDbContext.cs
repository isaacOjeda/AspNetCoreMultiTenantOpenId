using Finbuckle.MultiTenant.Stores;
using Microsoft.EntityFrameworkCore;
using MultiTenants.Web.Domain.Entities.TenantAdmin;

namespace MultiTenants.Web.Persistence;
public class TenantAdminDbContext : EFCoreStoreDbContext<MultiTenantInfo>
{
    public TenantAdminDbContext(DbContextOptions<TenantAdminDbContext> options)
        : base(options)
    {

    }
}
