using Microsoft.AspNetCore.Mvc;

namespace TechecomViet.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DoashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
