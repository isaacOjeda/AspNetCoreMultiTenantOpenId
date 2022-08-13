using Finbuckle.MultiTenant;

namespace MultiTenants.IdentityServer.Domain.Entities.TenantAdmin;
public class MultiTenantInfo : ITenantInfo
{
    public string Id { get; set; }
    public string Identifier { get; set; }
    public string Name { get; set; }
    public string ConnectionString { get; set; }
}
