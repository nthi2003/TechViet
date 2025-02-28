using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechecomViet.Models;
using TechecomViet.Models.ViewModel;
using TechecomViet.Reponsitory;

namespace TechecomViet.Controllers
{
   
    public class AccountController : BaseController
    {
        private readonly SignInManager<UserModel> _signInManager;
        private readonly UserManager<UserModel> _userManager;
        private readonly DataContext _dataContext;

        public AccountController(DataContext context ,SignInManager<UserModel> signInManager, UserManager<UserModel> userManager) : base(context)
        {
            _dataContext = context;
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
                        var roles = await _userManager.GetRolesAsync(user);
                        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email)
                };
                        foreach (var role in roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));
                        }
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await _signInManager.SignOutAsync(); 
                        await _signInManager.SignInAsync(user, isPersistent: false);

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
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            var checkMail = await _userManager.FindByEmailAsync(registerViewModel.Email);
            if (checkMail != null)
            {
                ModelState.AddModelError("", "Email đã tồn tại");
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

                var result = await _userManager.CreateAsync(user, registerViewModel.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");

                    var roles = await _userManager.GetRolesAsync(user);

                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await _signInManager.SignInAsync(user, isPersistent: false);

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
        [HttpGet]
        public async Task<IActionResult> AccountInfomation()
        {
            await SetCartItemCountAsync();
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null) {
                TempData["error"] = "Bạn chưa đăng nhập";
                return RedirectToAction("Index");
            }
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["error"] = "Người dùng không tồn tại";
                return RedirectToAction("Index");
            }

            var model = new UserModel
            {
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                Image = user.Image,
            };

            return View(model);
     
        }
        [HttpPost]
        public async Task<IActionResult> UpdateAccount(UserModel userModel)
        {
            await SetCartItemCountAsync();
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null)
            {
                TempData["error"] = "Bạn chưa đăng nhập";
                return RedirectToAction("AccountInfomation");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["error"] = "Người dùng không tồn tại";
                return RedirectToAction("AccountInfomation");
            }

            // Cập nhật thông tin user
            user.UserName = userModel.UserName;
            user.FullName = userModel.FullName;
            user.PhoneNumber = userModel.PhoneNumber;
            user.Address = userModel.Address;

            // Cập nhật ảnh nếu có
            if (userModel.ImageUpload != null && userModel.ImageUpload.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/accounts");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(user.Image))
                {
                    var oldFilePath = Path.Combine(folderPath, user.Image);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Lưu ảnh mới
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(userModel.ImageUpload.FileName)}";
                var filePath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await userModel.ImageUpload.CopyToAsync(stream);
                }

                user.Image = fileName; 
            }
            _dataContext.Update(user);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Cập nhật người dùng thành công";
            return RedirectToAction("AccountInfomation");
        }


    }
}
