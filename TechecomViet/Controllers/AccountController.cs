
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TechecomViet.Models;
using TechecomViet.Models.ViewModel;

namespace TechecomViet.Controllers
{
   
    public class AccountController : Controller
    {
        private readonly SignInManager<UserModel> _signInManager;
        private readonly UserManager<UserModel> _userManager;

        public AccountController(SignInManager<UserModel> signInManager, UserManager<UserModel> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View("Login"); 
        }


        [HttpPost]

        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(loginViewModel.Email);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, false, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home"); 
                    }
                    else
                    {
                        ModelState.AddModelError("", "Mật khẩu không chính xác.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Email không tồn tại.");
                }
            }
            return View(loginViewModel);
  
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View("Register");
        }
        [HttpPost]
        public async Task<IActionResult> Register( RegisterViewModel registerViewModel)
        {

            var CheckMail = await _userManager.FindByEmailAsync(registerViewModel.Email);
            if(CheckMail != null)
            {
                ModelState.AddModelError("", "Email đã tồn tại");
            }
            else
            {
                var user = new UserModel
                {
                    UserName = registerViewModel.UserName,
                    FullName = registerViewModel.FullName,
                    Email = registerViewModel.Email,
                    PhoneNumber = registerViewModel.PhoneNumber,
                
                    
                };
                var result = await _userManager.CreateAsync(user, registerViewModel.Password );
                if(result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");

                    await _signInManager.SignInAsync(user, isPersistent: false); // khi người dùng đóng trình duyệt thì sẽ xóa cookie
                    return RedirectToAction("Index", "Home");
                }


            }


            return View(registerViewModel);
        }
        
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
      


    }
}
