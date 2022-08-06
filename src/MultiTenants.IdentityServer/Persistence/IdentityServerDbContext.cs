using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MultiTenants.IdentityServer.Persistence;

public class IdentityServerDbContext : IdentityDbContext
{
    public IdentityServerDbContext(DbContextOptions<IdentityServerDbContext> options)
        : base(options)
    {
        
    }
}