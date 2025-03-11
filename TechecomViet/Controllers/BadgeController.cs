using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechecomViet.Models;
using TechecomViet.Models.ViewModel;
using TechecomViet.Reponsitory;

namespace TechecomViet.Controllers
{
    public class BadgeController : BaseController
    {

        private readonly DataContext _dataContext;

        public BadgeController(DataContext context) : base(context)
        {
            _dataContext = context;

        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
     

            await SetCartItemCountAsync();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _dataContext.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("Người dùng không tồn tại!");
            }

            var totalSum = await _dataContext.Orders
                .Where(o => o.UserId == userId)
                .SumAsync(o => o.TotalPrices);

            var model = new BadgeViewModel
            {
                UserName = user.UserName,
                TotalSum = totalSum,
                User = user
            };

            return View(model);
        }


    }

}
