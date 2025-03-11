using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechecomViet.Models;
using TechecomViet.Reponsitory;

namespace TechecomViet.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "ADMIN, BLOG")]
    public class BlogController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<UserModel> _userManager;
        public BlogController(DataContext context, UserManager<UserModel> userManager)
        {
            _dataContext = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Blogs.OrderByDescending(b => b.Id).ToListAsync());
        }
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost("Create")]
        public async Task<IActionResult> Create(BlogModel blog)
        {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userName = User.FindFirstValue(ClaimTypes.Name);

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userName))
                {
                    TempData["error"] = "Người dùng không hợp lệ.";
                    return RedirectToAction("Index", "Blog");
                }

                var checkTitle = await _dataContext.Blogs.FirstOrDefaultAsync(b => b.Title == blog.Title);
                if (checkTitle != null)
                {
                    TempData["error"] = "Bài viết đã tồn tại.";
                    return RedirectToAction("Index", "Blog");
                }

                if (blog.ImageUpload != null && blog.ImageUpload.Length > 0)
                {
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/blogs");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(blog.ImageUpload.FileName)}";
                    var filePath = Path.Combine(folderPath, fileName);

                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await blog.ImageUpload.CopyToAsync(fs);
                    }

                    blog.Image = fileName;
                }

                var newBlog = new BlogModel
                {
                    Title = blog.Title,
                    Description = blog.Description,
                    Status = blog.Status,
                    Author = userName,
                    Image = blog.Image,
                    CreatedDate = DateTime.Now
                };

                _dataContext.Add(newBlog);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Tạo bài viết thành công!";
                return RedirectToAction("Index", "Blog");
            }
        [Route("DeleteImage")]
        public async Task<IActionResult> DeleteImage(int blogId)
        {
            var blog = await _dataContext.Blogs.FindAsync(blogId);
            if (blog == null)
            {
                TempData["error"] = "Bài viết không tồn tại.";
                return RedirectToAction("Edit", new { id = blogId });
            }

            if (!string.IsNullOrEmpty(blog.Image))
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/blogs");
                var filePath = Path.Combine(folderPath, blog.Image);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                blog.Image = "";
                _dataContext.Blogs.Update(blog);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Xoá ảnh thành công.";
            }
            else
            {
                TempData["error"] = "Không có ảnh để xóa.";
            }

            return RedirectToAction("Edit", new { id = blogId });
        }
        [HttpGet("Edit")]
        public async Task<IActionResult> Edit(int Id)
        {
            var blog = await _dataContext.Blogs.FindAsync(Id);
            return View(blog);
        }
        [Route("Edit")]
        [HttpPost]
        public async Task<IActionResult> Edit(BlogModel blog)
        {

                var existingBlog = await _dataContext.Blogs.FindAsync(blog.Id);
                if (existingBlog == null)
                {
                    TempData["error"] = "Hãng không tồn tại!";
                    return RedirectToAction("Index");
                }
                if (blog.ImageUpload != null && blog.ImageUpload.Length > 0)
                {
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/blogs");
                    var oldFilePath = Path.Combine(folderPath, existingBlog.Image);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(blog.ImageUpload.FileName)}";
                    var filePath = Path.Combine(folderPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await blog.ImageUpload.CopyToAsync(stream);
                    }

                    existingBlog.Image = fileName;
                }
                existingBlog.Title = blog.Title;
                existingBlog.Description = blog.Description;
                existingBlog.Status = blog.Status;
                _dataContext.Blogs.Update(existingBlog);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Cập nhật bài viết thành công.";
            return RedirectToAction("Index");
        }
        [Route("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var blog = await _dataContext.Blogs.FindAsync(id);
            if (blog == null)
            {
                TempData["error"] = "Không tìm thấy bài viết.";
            }
            if (!string.IsNullOrEmpty(blog.Image))
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/blogs");
                var filePath = Path.Combine(folderPath, blog.Image);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            _dataContext.Blogs.Remove(blog);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Xóa bài viết thành công";
            return RedirectToAction("Index");
        }

    }
}
