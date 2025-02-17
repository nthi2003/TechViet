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
    public class SliderController : Controller
    {

        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnviroment;
        public SliderController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnviroment = webHostEnvironment;
        }
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Sliders.OrderByDescending(p => p.Id).ToListAsync());
        }
        [Route("Create")]
        public IActionResult Create()
        {

            return View();
        }
        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SliderModel slider)
        {

            if (slider.ImageUpload != null && slider.ImageUpload.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/sliders");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(slider.ImageUpload.FileName)}";
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await slider.ImageUpload.CopyToAsync(stream);
                }

                slider.Image = $"{fileName}";
            }


            _dataContext.Add(slider);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Thêm quảng cáo thành công";
            return RedirectToAction("Index");

        }
        [Route("DeleteImage")]
        public async Task<IActionResult> DeleteImage(int sliderId)
        {
            var slider = await _dataContext.Sliders.FindAsync(sliderId);
            if (slider == null)
            {
                TempData["error"] = "Quảng cáo không tồn tại.";
                return RedirectToAction("Edit", new { id = sliderId });
            }

            if (!string.IsNullOrEmpty(slider.Image))
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/sliders");
                var filePath = Path.Combine(folderPath, slider.Image);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                slider.Image = "";
                _dataContext.Sliders.Update(slider);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Xoá ảnh thành công.";
            }
            else
            {
                TempData["error"] = "Không có ảnh để xóa.";
            }

            return RedirectToAction("Edit", new { id = sliderId });
        }


        [Route("Edit")]

        public async Task<IActionResult> Edit(int Id)
        {
            var slider = await _dataContext.Sliders.FindAsync(Id);

            return View(slider);
        }
        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SliderModel slider)
        {
            var slider_existed = _dataContext.Sliders.Find(slider.Id);

            if (slider.ImageUpload != null)
            {
                string uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/sliders");
                string imageName = Guid.NewGuid().ToString() + "_" + slider.ImageUpload.FileName;
                string filePath = Path.Combine(uploadsDir, imageName);

                FileStream fs = new FileStream(filePath, FileMode.Create);
                await slider.ImageUpload.CopyToAsync(fs);
                fs.Close();
                slider_existed.Image = imageName;
            }
            slider_existed.Name = slider.Name;
            slider_existed.Description = slider.Description;
            slider_existed.Status = slider.Status;


            _dataContext.Update(slider_existed);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Cập nhật quảng cáo thành công";
            return RedirectToAction("Index");

        }

        [Route("Delete")]

        public async Task<IActionResult> Delete(int Id)
        {
            var slider = await _dataContext.Sliders.FindAsync(Id);
            if (!string.Equals(slider.Image, "noname.jpg"))
            {
                string uploadsDir = Path.Combine(_webHostEnviroment.WebRootPath, "wwwroot/sliders");
                string oldfilePath = Path.Combine(uploadsDir, slider.Image);
                if (System.IO.File.Exists(oldfilePath))
                {
                    System.IO.File.Delete(oldfilePath);
                }
            }

            _dataContext.Sliders.Remove(slider);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Slider đã được xóa thành công";
            return RedirectToAction("Index");
        }
    }
}
