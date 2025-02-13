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
    public class CouponController : Controller
    {
        private readonly DataContext _dataContext;
        public CouponController(DataContext context)
        {
            _dataContext = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var coupon_list = await _dataContext.Coupons.ToListAsync();
            ViewBag.Coupons = coupon_list;
            return View();
        }
        [Route("Create")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
           return View();
        }
        [Route("Create")]
        [HttpPost]
        public async Task<IActionResult> Create(CouponModel coupon)
        {
            _dataContext.Add(coupon);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Thêm mã giảm giá thành công";
            return RedirectToAction("Index");
        }
        [Route("Edit")]
        [HttpGet]
        public async Task<IActionResult> Edit (int id)
        {
            var coupon = await _dataContext.Coupons.FindAsync(id);
            return View(coupon);
        }
        [Route("Edit")]
        [HttpPost]
        public async Task<IActionResult> Edit(CouponModel coupon)
        {
            var existingCoupon = await _dataContext.Coupons.FindAsync(coupon.Id);
            if (existingCoupon != null) {
                TempData["error"] = "Mã giảm giá không tồn tại";
            }
            existingCoupon.Name = coupon.Name;
            existingCoupon.Description = coupon.Description;
            existingCoupon.DateStart = coupon.DateStart;
            existingCoupon.DateExpired = coupon.DateExpired;
            existingCoupon.Status = coupon.Status;
            _dataContext.Coupons.Update(existingCoupon);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Cập nhật mã giảm giá.";
            return RedirectToAction("Index");
        }

    }
}
