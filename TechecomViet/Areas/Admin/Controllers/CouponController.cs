using Microsoft.AspNetCore.Mvc;

namespace TechecomViet.Areas.Admin.Controllers
{
    public class CouponController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
