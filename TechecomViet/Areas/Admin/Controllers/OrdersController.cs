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
        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 15;
            int pageNumber = (page ?? 1);
            int skip = (pageNumber - 1) * pageSize;
            int totalItems = await _dataContext.Orders.CountAsync();
            var orders = await _dataContext.Orders
                                          .OrderByDescending(o => o.Id)
                                          .Skip(skip)
                                          .Take(pageSize)
                                          .ToListAsync();
            ViewBag.TotalItems = totalItems;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;

            return View(orders);
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
        public async Task<IActionResult> UpdateOrderAdmin(int Id, int Status)
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
