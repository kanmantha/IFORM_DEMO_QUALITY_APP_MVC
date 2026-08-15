using IFormQualityApp.Data;
using IFormQualityApp.Models.Entities;
using IFormQualityApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IFormQualityApp.Controllers;

[Authorize]
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _db;

    public ProductsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? category)
    {
        IQueryable<Product> query = _db.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                p.Code.Contains(term) ||
                p.Name.Contains(term) ||
                (p.Category != null && p.Category.Contains(term)) ||
                (p.Material != null && p.Material.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category == category);
        }

        var categories = await _db.Products
            .Where(p => p.Category != null)
            .Select(p => p.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        var products = await query
            .OrderBy(p => p.Code)
            .ToListAsync();

        var vm = new ProductListViewModel
        {
            Search = search,
            Category = category,
            Categories = categories,
            Products = products
        };

        ViewData["ActiveMenu"] = "Products";
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var product = await _db.Products
            .Include(p => p.Project)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        ViewData["ActiveMenu"] = "Products";
        return View(product);
    }

    /// <summary>
    /// Generates an SVG placeholder image for a product card (used until real photos
    /// are uploaded). Color is derived from the product code.
    /// </summary>
    [HttpGet]
    [ResponseCache(Duration = 86400)]
    public async Task<IActionResult> Image(int id, int size = 400)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            return NotFound();
        }

        var code = System.Security.SecurityElement.Escape(product.Code);
        var name = System.Security.SecurityElement.Escape(product.Name);
        var category = System.Security.SecurityElement.Escape(product.Category ?? "Accessories");

        var hue = (int)(uint)product.Code.GetHashCode() % 360;
        if (hue < 0) hue = -hue;

        var svg = $@"<svg xmlns=""http://www.w3.org/2000/svg"" width=""{size}"" height=""{size}"">
  <defs>
    <linearGradient id=""g"" x1=""0"" y1=""0"" x2=""1"" y2=""1"">
      <stop offset=""0%"" stop-color=""hsl({hue},45%,85%)""/>
      <stop offset=""100%"" stop-color=""hsl({hue},55%,65%)""/>
    </linearGradient>
  </defs>
  <rect width=""100%"" height=""100%"" fill=""url(#g)""/>
  <rect width=""100%"" height=""100%"" fill=""none"" stroke=""hsl({hue},50%,45%)"" stroke-width=""2""/>
  <circle cx=""50%"" cy=""42%"" r=""{size * 0.16}"" fill=""rgba(255,255,255,0.35)""/>
  <circle cx=""50%"" cy=""42%"" r=""{size * 0.11}"" fill=""rgba(255,255,255,0.55)""/>
  <rect x=""{size * 0.18}"" y=""{size * 0.66}"" width=""{size * 0.64}"" height=""{size * 0.06}"" rx=""4"" fill=""rgba(255,255,255,0.85)""/>
  <rect x=""{size * 0.30}"" y=""{size * 0.78}"" width=""{size * 0.40}"" height=""{size * 0.045}"" rx=""3"" fill=""rgba(255,255,255,0.6)""/>
  <text x=""50%"" y=""50%"" text-anchor=""middle"" font-family=""Arial, sans-serif"" font-size=""{size * 0.09}"" font-weight=""bold"" fill=""hsl({hue},45%,28%)"">{code}</text>
  <text x=""50%"" y=""{size * 0.91}"" text-anchor=""middle"" font-family=""Arial, sans-serif"" font-size=""{size * 0.045}"" fill=""rgba(0,0,0,0.55)"">{category}</text>
</svg>";

        return Content(svg, "image/svg+xml");
    }
}
