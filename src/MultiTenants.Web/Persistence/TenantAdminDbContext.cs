using Finbuckle.MultiTenant.Stores;
using Microsoft.EntityFrameworkCore;

namespace MultiTenants.Web.Persistence;
public class TenantAdminDbContext : EFCoreStoreDbContext<MultiTenantInfo>
{
    public TenantAdminDbContext(DbContextOptions<TenantAdminDbContext> options)
        : base(options)
    {

    }
}
