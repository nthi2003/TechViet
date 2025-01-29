using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechecomViet.Reponsitory;

namespace TechecomViet.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        public ProductController(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Products.OrderByDescending(p => p.Id).Include(c => c.Category).Include(b => b.Brand).ToListAsync());
        }
        [Route("Create")]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
            return View();
        }

        [Route("Create")]
        [HttpPost]
        public async Task<IActionResult> Create(ProductModel product)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            if (ModelState.IsValid)
            {
                var checkProduct = await _dataContext.Products.FirstOrDefaultAsync(p => p.Name == product.Name);
                if (checkProduct != null)
                {
                    TempData["error"] = "Sản phẩm đã tồn tại.";
                    return View(product);
                }
                List<string> imageNames = new List<string>();
                if (product.ImageUploads != null && product.ImageUploads.Count > 0)
                {
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "images/product");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    foreach (var image in product.ImageUploads)
                    {
                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                        var filePath = Path.Combine(folderPath, fileName);
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(fs);
                        }
                        imageNames.Add(fileName);
                    }
                }

                product.Images = imageNames;

                _dataContext.Products.Add(product);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Sản phẩm đã được tạo thành công!";
                return RedirectToAction("Index");
            }

            return View(product);
        }



    }
}
