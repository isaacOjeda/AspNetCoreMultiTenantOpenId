using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MultiTenants.IdentityServer.Domain.Entities.TenantAdmin;

namespace MultiTenants.IdentityServer.Persistence;

public class IdentityServerDbContext : IdentityDbContext
{
    private readonly MultiTenantInfo _tenant;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    public IdentityServerDbContext(
        DbContextOptions<IdentityServerDbContext> options,
        IMultiTenantContextAccessor<MultiTenantInfo> accessor,
        IWebHostEnvironment env,
        IConfiguration config) : base(options)
    {
        _tenant = accessor.MultiTenantContext?.TenantInfo;
        _env = env;
        _config = config;
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString;

        if (_tenant is null && _env.IsDevelopment())
        {
            // Init/Dev connection string
            connectionString = _config.GetConnectionString("DefaultConnection");
        }
        else
        {
            // Tenant connection string
            connectionString = _tenant.ConnectionString;
        }

        optionsBuilder.UseSqlServer(connectionString);
        optionsBuilder.UseOpenIddict();


        base.OnConfiguring(optionsBuilder);
    }
}