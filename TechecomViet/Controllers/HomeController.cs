using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;



namespace TechecomViet.Controllers
{
    
    public class HomeController : Controller
    {
        
        public IActionResult Index()
        {
          return View();
        }


    }
}
