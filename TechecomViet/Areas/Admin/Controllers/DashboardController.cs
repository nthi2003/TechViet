using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var count_category = _dataContext.Categories.Count();
            var count_blog = _dataContext.Blogs.Count();
            var count_brand = _dataContext.Brands.Count();
            var totalRevenue = _dataContext.Orders
                .Where(o => o.Status == 5 || o.Status == 6)
                .Sum(o => o.TotalPrices);
            ViewBag.CountCategory = count_category;
            ViewBag.CountBlog = count_blog;
            ViewBag.CountBrand = count_brand;
            ViewBag.CountProduct = count_product;
            ViewBag.CountOrder = count_order;
            ViewBag.CountUser = count_user;
            ViewBag.TotalRevenue = totalRevenue;

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
