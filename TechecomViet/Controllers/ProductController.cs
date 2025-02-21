using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechecomViet.Reponsitory;

namespace TechecomViet.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        public ProductController(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _dataContext.Products
                                .Include(p => p.Brand)
                                .FirstOrDefaultAsync(p => p.Id == id);
            var relatedProducts = await _dataContext.Products
                .Where(p => p.Id != product.Id && (p.CategoryId == product.CategoryId || p.BrandId == product.BrandId))
                .Take(4)
                .ToListAsync(); ;
            ViewBag.RelatedProducts = relatedProducts;
            return View(product);
        }
    }
}
