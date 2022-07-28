namespace MultiTenants.Web.Domain.Entities.TenantAdmin;
public class Tenant
{
    public int TenantId { get; set; }
    public string Name { get; set; } = default!;
    public string Identifier { get; set; } = default!;
    public string ConnectionString { get; set; } = default!;
}
