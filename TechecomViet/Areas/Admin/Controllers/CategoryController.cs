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
    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;
        public CategoryController(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IActionResult> Index(string? name)
        {
            var category = _dataContext.Categories.AsQueryable();
            if(name != null)
            {
                category = category.Where(c => c.Name.Contains(name));
            }
            return View(await category.OrderByDescending(c => c.Id).ToListAsync());
        }
        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }
        [Route("Create")]
        [HttpPost]
        public async Task<IActionResult> Create(CategoryModel category)
        {

            var existingCategory = await _dataContext.Categories.FirstOrDefaultAsync(c => c.Name == category.Name);
            if (existingCategory != null)
            {
                ModelState.AddModelError("", "Danh mục đã tồn tại");
                return View(category);
            }
            if (category.ImageUpload != null && category.ImageUpload.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/categories");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(category.ImageUpload.FileName)}";
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await category.ImageUpload.CopyToAsync(stream);
                }

                category.Image = $"{fileName}";
            }

            _dataContext.Add(category);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Thêm danh mục thành công";
            return RedirectToAction("Index");
        }
        [Route("DeleteImage")]
        public async Task<IActionResult> DeleteImage(int categoryId)
        {
            var category = await _dataContext.Categories.FindAsync(categoryId);
            if (category == null)
            {
                TempData["error"] = "Hãng không tồn tại.";
                return RedirectToAction("Edit", new { id = categoryId });
            }

            if (!string.IsNullOrEmpty(category.Image))
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/categories");
                var filePath = Path.Combine(folderPath, category.Image);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                category.Image = "";
                _dataContext.Categories.Update(category);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Xoá ảnh thành công.";
            }
            else
            {
                TempData["error"] = "Không có ảnh để xóa.";
            }

            return RedirectToAction("Edit", new { id = categoryId });
        }

        [Route("Edit")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _dataContext.Categories.FindAsync(id);
            return View(category);
        }
        [Route("Edit")]
        [HttpPost]
        public async Task<IActionResult> Edit(CategoryModel category)
        {

            var existingCategory = await _dataContext.Categories.FindAsync(category.Id);
            if (existingCategory == null)
            {
                TempData["error"] = "Thư mục không tồn tại!";
                return RedirectToAction("Index");
            }
            if (category.ImageUpload != null && category.ImageUpload.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/categories");
                var oldFilePath = Path.Combine(folderPath, existingCategory.Image);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(category.ImageUpload.FileName)}";
                var filePath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await category.ImageUpload.CopyToAsync(stream);
                }

                existingCategory.Image = fileName;
            }
            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            existingCategory.Status = category.Status;
            _dataContext.Categories.Update(existingCategory);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Cập nhật thư mục thành công.";
            return RedirectToAction("Index");
        }

        [Route("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _dataContext.Categories.FindAsync(id);
            if (!string.IsNullOrEmpty(category.Image))
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/categories");
                var filePath = Path.Combine(folderPath, category.Image);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            _dataContext.Categories.Remove(category);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Xóa thư mục thành công";
            return RedirectToAction("Index");
        }
    }
}
