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
    public class OrdersController : Controller
    {
        private readonly DataContext _dataContext;  
        public OrdersController(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Orders.OrderByDescending(o => o.Id).ToListAsync());
        }
        [HttpGet("OrderDetail/{Id}")]
        public async Task<IActionResult> OrderDetail(int Id)
        {
            var orderItems = await _dataContext.OrderItems
             .Where(o => o.OrderId == Id)
             .ToListAsync();

            return View(orderItems);
        }
        [HttpPost]
        [Route("UpdateOrder/{Id}")]
        public async Task<IActionResult> UpdateOrder(int Id, int Status)
        {
            var checkOrder = await _dataContext.Orders.FirstOrDefaultAsync(o => o.Id == Id);
            if (checkOrder == null)
            {
                TempData["error"] = "Đơn hàng không tồn tại";
                return RedirectToAction("Index");
            }

            checkOrder.Status = Status;
            _dataContext.Update(checkOrder);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Cập nhật đơn hàng thành công";
            return RedirectToAction("Index");
        }
    }
}
