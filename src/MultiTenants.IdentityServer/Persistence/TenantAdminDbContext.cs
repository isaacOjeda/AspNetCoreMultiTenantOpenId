using Finbuckle.MultiTenant.Stores;
using Microsoft.EntityFrameworkCore;
using MultiTenants.IdentityServer.Domain.Entities.TenantAdmin;

namespace MultiTenants.IdentityServer.Persistence;


public class TenantAdminDbContext : EFCoreStoreDbContext<MultiTenantInfo>
{
    public TenantAdminDbContext(DbContextOptions<TenantAdminDbContext> options)
        : base(options)
    {

    }
}
