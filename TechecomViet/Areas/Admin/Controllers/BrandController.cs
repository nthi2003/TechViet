using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechecomViet.Models;
using TechecomViet.Reponsitory;

namespace TechecomViet.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;
        public BrandController(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IActionResult> Index(string? name)
        {
            var brand = _dataContext.Brands.AsQueryable();
            if(name != null)
            {
                brand = brand.Where(b => b.Name.Contains(name));
            }
            return View(await brand.OrderByDescending(b => b.Id).ToListAsync());
        }
        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }
        [Route("Create")]
        [HttpPost]
        public async Task<IActionResult> Create(BrandModel brand)
         {

                var existingBrand = await _dataContext.Brands.FirstOrDefaultAsync(b => b.Name == brand.Name);
                if (existingBrand != null)
                {
                    TempData["error"] = "Hãng đã tồn tại.";
                    return View(brand);
                }
                if (brand.ImageUpload != null )
                {
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/brands");
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(brand.ImageUpload.FileName)}";
                    var filePath = Path.Combine(folderPath, fileName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await brand.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    brand.Image = fileName;
                }

                _dataContext.Add(brand);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm hãng thành công";
                return RedirectToAction("Index");

        }
        [Route("DeleteImage")]
        public async Task<IActionResult> DeleteImage(int brandId)
        {
            var brand = await _dataContext.Brands.FindAsync(brandId);
            if (brand == null)
            {
                TempData["error"] = "Hãng không tồn tại.";
                return RedirectToAction("Edit", new { id = brandId });
            }

            if (!string.IsNullOrEmpty(brand.Image))
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/brands");
                var filePath = Path.Combine(folderPath, brand.Image);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                brand.Image = "";
                _dataContext.Brands.Update(brand);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Xoá ảnh thành công.";
            }
            else
            {
                TempData["error"] = "Không có ảnh để xóa.";
            }

            return RedirectToAction("Edit", new { id = brandId });
        }

        [Route("Edit")]
        [HttpGet]
        public async Task<IActionResult>Edit(int id)
        {
            var brand = await _dataContext.Brands.FindAsync(id);
            return View(brand);
        }
        [Route("Edit")]
        [HttpPost]
        public async Task<IActionResult> Edit(BrandModel brand)
        {
                var existingBrand = await _dataContext.Brands.FindAsync(brand.Id);
                if (existingBrand == null)
                {
                    TempData["error"] = "Hãng không tồn tại!";
                    return RedirectToAction("Index");
                }
                if (brand.ImageUpload != null && brand.ImageUpload.Length > 0)
                {
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/brands");
                    var oldFilePath = Path.Combine(folderPath, existingBrand.Image);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(brand.ImageUpload.FileName)}";
                    var filePath = Path.Combine(folderPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await brand.ImageUpload.CopyToAsync(stream);
                    }

                    existingBrand.Image = fileName;
                }
                existingBrand.Name = brand.Name;
                existingBrand.Description = brand.Description;
                existingBrand.Status = brand.Status;
                _dataContext.Brands.Update(existingBrand);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Cập nhật thương hiệu thành công.";
                return RedirectToAction("Index");
        }
        [Route("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _dataContext.Brands.FindAsync(id);
            if (brand == null) {
                TempData["error"] = "Không tìm thấy thương hiệu.";
            }
            if (!string.IsNullOrEmpty(brand.Image))
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/brands");
                var filePath = Path.Combine(folderPath, brand.Image);
                if (System.IO.File.Exists(filePath)) {
                    System.IO.File.Delete(filePath);
                }
            }    
            _dataContext.Brands.Remove(brand);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Xóa hãng thành công";
            return RedirectToAction("Index");
        }

    }
}
