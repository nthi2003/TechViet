using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechecomViet.Models;
using TechecomViet.Models.ViewModel;
using TechecomViet.Reponsitory;

namespace TechecomViet.Controllers
{
    public class ProductController : BaseController
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<UserModel> _userManager;
        public ProductController(DataContext context, UserManager<UserModel> userManager) : base(context)
        {
            _dataContext = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Detail(int id)
        {
            await SetCartItemCountAsync();

            var product = await _dataContext.Products
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var relatedProducts = await _dataContext.Products
                .Where(p => p.Id != product.Id && (p.CategoryId == product.CategoryId || p.BrandId == product.BrandId))
                .Take(4)
                .ToListAsync();

            var reviews = await _dataContext.Reviews
                .Where(r => r.ProductId == id)
                .Include(r => r.User)
                .ToListAsync();

            var checkUser = await _userManager.GetUserAsync(User);
            bool checkReview = false; 

            if (checkUser != null)
            {
                checkReview = await _dataContext.Orders
                    .Include(o => o.OrderItems)
                    .AnyAsync(o => o.UserId == checkUser.Id &&
                                   o.Status == 5 &&
                                   o.OrderItems.Any(oi => oi.ProductName == product.Name));
            }

            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                Reviews = reviews,
                Users = checkUser,
                CheckReview = checkReview
            };

            ViewBag.RelatedProducts = relatedProducts;

            return View(viewModel);
        }
       
        [HttpPost]
        public async Task<IActionResult> Search(string searchTerm)
        {
            await SetCartItemCountAsync();
            var product = await _dataContext.Products.Where(p => p.Name.Contains(searchTerm)).ToListAsync();
            ViewBag.Keyword = searchTerm;
            return View(product);
        }
        [HttpGet("FindProductCategory")]
        public async Task<IActionResult> FindProductCategory(int categoryId, int? brandId, int? minPrice, int? maxPrice)
        {
            await SetCartItemCountAsync();
            var brands = await _dataContext.Products
                .Where(p => p.CategoryId == categoryId)
                .Select(p => p.Brand)
                .Distinct()
                .ToListAsync();

            var category = await _dataContext.Categories
                .FirstOrDefaultAsync(p => p.Id == categoryId);
            var query = _dataContext.Products.Where(p => p.CategoryId == categoryId);

            if (brandId.HasValue)
            {
                query = query.Where(p => p.BrandId == brandId.Value);
            }
            if (minPrice.HasValue && minPrice.Value > 0)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue && maxPrice.Value > 0)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            var products = await query.ToListAsync();

            ViewBag.Brands = brands;
            ViewBag.Category = category;
            ViewBag.SelectedBrandId = brandId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(products);
        }

    }
}
