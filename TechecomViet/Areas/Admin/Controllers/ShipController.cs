using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using TechecomViet.Models;
using TechecomViet.Reponsitory;

namespace TechecomViet.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class ShipController : Controller
    {
        private readonly DataContext _dataContext;
        public ShipController(DataContext context)
        {
            _dataContext = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var shippingList = await _dataContext.Shippings.ToListAsync();
            return View(shippingList);
        }
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            return View();
        }
        [Route("Create")]
        [HttpPost]
        public async Task<IActionResult> Create(ShippingModel shippingModel , string phuong, string quan , string tinh , int price)
        {
            shippingModel.City = tinh;
            shippingModel.District = quan;
            shippingModel.Ward = phuong;
            shippingModel.Price = price;
            var existingShipping = await _dataContext.Shippings.AnyAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);
            if (existingShipping)
            {
                TempData["error"] = "Giá khu vực này đã tồn tại";
                return RedirectToAction("Index");
            }
            _dataContext.Shippings.Add(shippingModel);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Thêm giá giao hàng theo khu vực thành công";
            return RedirectToAction("Index");
        }

        [Route("Delete")]
        public async Task<IActionResult>Delete(int Id)
        {
            var shipping = await _dataContext.Shippings.FindAsync(Id);
            _dataContext.Shippings.Remove(shipping);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Shipping đã được xóa thành công";
            return RedirectToAction("Index");

        }
    }
}
