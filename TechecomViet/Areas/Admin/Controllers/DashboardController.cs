using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechecomViet.Reponsitory;

namespace TechecomViet.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "ADMIN,STAFF,BLOG")]
    public class DashboardController : Controller
    {
        private readonly DataContext _dataContext;
        public DashboardController(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IActionResult> Index()
        {
            var count_product = _dataContext.Products.Count();
            var count_order = _dataContext.Orders.Count();
            var count_user = _dataContext.Users.Count();

            ViewBag.CountProduct = count_product;
            ViewBag.CountOrder = count_order;
            ViewBag.CountUser = count_user;

            return View();
        }


        [HttpGet]
        [Route("GetStatisticsMonth")]
        public IActionResult GetStatisticsMonth(string filterMonth)
        {
 
            var selectedDate = DateTime.ParseExact(filterMonth, "yyyy-MM", null);
            var daysInMonth = DateTime.DaysInMonth(selectedDate.Year, selectedDate.Month);

  
            var statistics = _dataContext.Orders
                .Where(o => o.CreatedDate.Year == selectedDate.Year && o.CreatedDate.Month == selectedDate.Month)
                .GroupBy(o => o.CreatedDate.Day)
                .Select(g => new
                {
                    date = $"{selectedDate.Year}-{selectedDate.Month:00}-{g.Key:00}",
                    totalRevenue = g.Sum(o => o.TotalPrices)
                })
                .ToList();

       
            var fullStatistics = Enumerable.Range(1, daysInMonth)
                .Select(day => new
                {
                    date = $"{selectedDate.Year}-{selectedDate.Month:00}-{day:00}",
                    totalRevenue = statistics.FirstOrDefault(s => s.date.EndsWith($"-{day:00}"))?.totalRevenue ?? 0
                })
                .ToList();

            return Ok(fullStatistics);
        }


    }
}
