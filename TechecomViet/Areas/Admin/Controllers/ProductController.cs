using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechecomViet.Models;
using TechecomViet.Reponsitory;

namespace TechecomViet.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnviroment;
        public ProductController(DataContext context , IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnviroment = webHostEnvironment;
        }
        public async Task<IActionResult> Index(string? name)
        {
            var products = _dataContext.Products.AsQueryable();
            if (name != null)
            {
                products = products.Where(b => b.Name.Contains(name));
            }
            return View(await products.OrderByDescending(p => p.Id).Include(c => c.Category).Include(b => b.Brand).ToListAsync());
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
            var checkProduct = await _dataContext.Products.FirstOrDefaultAsync(p => p.Name == product.Name);
            if (checkProduct != null) {
                TempData["error"] = "Sản phẩm đã tồn tại";
                return View(checkProduct);

            }
            if (product.ImageUploads != null && product.ImageUploads.Count > 0)
            {
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/products");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                List<string> imageNames = new List<string>();

                foreach (var image in product.ImageUploads)
                {
                    string imageName = $"{Guid.NewGuid()}_{image.FileName}";
                    string filePath = Path.Combine(folderPath, imageName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    imageNames.Add(imageName);
                }
                product.Images = imageNames;
            }

            _dataContext.Products.Add(product);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Thêm sản phẩm thành công";
            return RedirectToAction("Index");
        }
        [Route("DeleteImage")]
        public async Task<IActionResult> DeleteImage(int productId, string imageName)
        {

            var product = await _dataContext.Products.FindAsync(productId);

            if (product.Images == null || product.Images.Count == 0)
            {
                TempData["error"] = "Không có ảnh để xóa.";
                return RedirectToAction("Edit", new { id = productId });
            }

            if (string.IsNullOrEmpty(imageName))
            {
                TempData["error"] = "Vui lòng chọn tên ảnh cần xóa.";
                return RedirectToAction("Edit", new { id = productId });
            }

            if (!product.Images.Contains(imageName))
            {
                TempData["error"] = "Ảnh không tồn tại trong danh sách của sản phẩm.";
                return RedirectToAction("Edit", new { id = productId });
            }

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/products");
            var filePath = Path.Combine(folderPath, imageName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            product.Images.Remove(imageName);
            _dataContext.Products.Update(product);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Xóa ảnh thành công.";
            return RedirectToAction("Edit", new { id = productId });
        }
        [Route("Edit")]
        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            var product = await _dataContext.Products.FindAsync(Id);
            if(product == null)
            {
                TempData["error"] = "Không tìm thấy sản phẩm";
                return RedirectToAction("Index");
            }
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");

            return View(product);
        }

        [Route("Edit")]
        [HttpPost]
        public async Task<IActionResult> Edit(ProductModel product)
        {
            var existed_product = await _dataContext.Products.FindAsync(product.Id);

            if (existed_product == null)
            {
                TempData["error"] = "Sản phẩm không tồn tại";
            }

            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);
            if (product.ImageUploads != null && product.ImageUploads.Count > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/products");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                if (existed_product.Images == null)
                {
                    existed_product.Images = new List<string>();
                }

                foreach (var image in product.ImageUploads)
                {
                    string imageName = $"{Guid.NewGuid()}_{image.FileName}";
                    string filePath = Path.Combine(folderPath, imageName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    existed_product.Images.Add(imageName);
                }
            }

            existed_product.Name = product.Name;
            existed_product.DiscountPercentage = product.DiscountPercentage;
            existed_product.Description = product.Description;
            existed_product.Price = product.Price;
            existed_product.CategoryId = product.CategoryId;
            existed_product.BrandId = product.BrandId;

            _dataContext.Update(existed_product);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Cập nhật sản phẩm thành công";
            return RedirectToAction("Index");
        }



        [Route("Delete")]
        public async Task <IActionResult> Delete(int id)
        {
            var product = await _dataContext.Products.FindAsync(id);
            if(product == null)
            {
                TempData["error"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction("Index");
            }
            if (product.Images != null && product.Images.Count > 0) {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/products");
                foreach (var image in product.Images) {
                    var filePath = Path.Combine(folderPath, image);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }
            _dataContext.Products.Remove(product);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Xóa sản phẩm thành công";
            return RedirectToAction("Index");


        }
        [Route("AddQuantity")]
        [HttpGet]
        public async Task<IActionResult> AddQuantity(int Id)
        {
            var productQuantity = await _dataContext.ProductQuantities.Where(pq => pq.ProductId == Id).ToListAsync();
            ViewBag.ProductByQuantity = productQuantity; 
            ViewBag.ProductId = Id;
            return View();
        }
        [Route("AddQuantity")]
        [HttpPost]
        public IActionResult AddQuantity(ProductQuantityModel productQuantityModel)
        {
            var product = _dataContext.Products.Find(productQuantityModel.ProductId);
            if (product == null)
            {
                return NotFound();
            }

            product.Quantity += productQuantityModel.Quantity;
            productQuantityModel.Quantity = productQuantityModel.Quantity;
            productQuantityModel.ProductId = productQuantityModel.ProductId;
            productQuantityModel.DateCreated = DateTime.Now;

            _dataContext.Add(productQuantityModel);
            _dataContext.SaveChangesAsync();
            TempData["Success"] = "Thêm số lượng sản phẩm thành công";
            return RedirectToAction("AddQuantity", "Product");
        }
    }
}
