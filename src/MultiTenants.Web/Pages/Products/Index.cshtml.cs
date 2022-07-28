using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MultiTenants.Web.Domain.Entities;
using MultiTenants.Web.Persistence;

namespace MultiTenants.Web.Pages.Products;

public class IndexModel : PageModel
{
    private readonly MyDbContext _context;

    public IndexModel(MyDbContext context)
    {
        _context = context;
    }


    public ICollection<Product> Products { get; set; } = new List<Product>();


    public async Task OnGet()
    {
        Products = await _context.Products.ToListAsync();
    }
}

