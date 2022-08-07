using Finbuckle.MultiTenant;

namespace MultiTenants.Web;
public class MultiTenantInfo : ITenantInfo
{
    public string Id { get; set; }
    public string Identifier { get; set; }
    public string Name { get; set; }
    public string ConnectionString { get; set; }
    public string OpenIdClientId { get; set; }
    public string OpenIdClientSecret { get; set; }
    public string OpenIdAuthority { get; set; }
}
