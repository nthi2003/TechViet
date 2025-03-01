using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechecomViet.Models;
using TechecomViet.Reponsitory;

namespace TechecomViet.Controllers
{
    public class OrderController : BaseController
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<UserModel> _userManager;
        public OrderController(DataContext context, UserManager<UserModel> userManager) : base(context)
        {
            _dataContext = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> MyOrder()
        {
            await SetCartItemCountAsync();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }
            var order = await _dataContext.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ToListAsync();
            return View(order);
        }
        public async Task<IActionResult> UpdateOrder(int Id)
        {

            var checkOrder = await _dataContext.Orders.FirstOrDefaultAsync(o => o.Id == Id);
            if (checkOrder == null)
            {
                TempData["error"] = "Đơn hàng không tồn tại";
                return RedirectToAction("MyOrder");
            }
            checkOrder.Status =  0;
            _dataContext.Update(checkOrder);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Hủy đơn thành công";
            return RedirectToAction("MyOrder");
        }
    }
}
