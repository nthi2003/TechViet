using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
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

        public AccountController(DataContext context, SignInManager<UserModel> signInManager, UserManager<UserModel> userManager) : base(context)
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


        public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            await _signInManager.SignOutAsync(); // thoat bang gg
            await HttpContext.SignOutAsync();
            return Redirect(returnUrl);
        }
        [HttpGet]
        public async Task<IActionResult> AccountInfomation()
        {
            await SetCartItemCountAsync();
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null)
            {
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

            user.UserName = userModel.UserName;
            user.FullName = userModel.FullName;
            user.PhoneNumber = userModel.PhoneNumber;
            user.Address = userModel.Address;

            if (userModel.ImageUpload != null && userModel.ImageUpload.Length > 0)
            {

                if (!string.IsNullOrEmpty(user.Image))
                {
                    var oldFilePath = Path.Combine("wwwroot/accounts", user.Image);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(userModel.ImageUpload.FileName)}";
                var filePath = Path.Combine("wwwroot/accounts", fileName);
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

        public async Task LoginByGoogle()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("GoogleResponse")
                });
        }
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                return RedirectToAction("Login");
            }

            var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claim => new
            {
                claim.Issuer,
                claim.OriginalIssuer,
                claim.Type,
                claim.Value
            });

            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            string emailName = email.Split('@')[0];

            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser == null)
            {
                var passwordHasher = new PasswordHasher<UserModel>();
                var hashedPassword = passwordHasher.HashPassword(null, "123456789");

                var newUser = new UserModel
                {
                    UserName = emailName,
                    Email = email
                };
                newUser.PasswordHash = hashedPassword;

                var createUserResult = await _userManager.CreateAsync(newUser);
                if (!createUserResult.Succeeded)
                {
                    TempData["error"] = "Đăng ký tài khoản thất bại. Vui lòng thử lại sau.";
                    return RedirectToAction("Login", "Account");
                }

                var userRoles = await _userManager.GetRolesAsync(newUser);
                if (!userRoles.Contains("User"))
                {
                    await _userManager.AddToRoleAsync(newUser, "User");
                }

                await _signInManager.SignInAsync(newUser, isPersistent: false);
                TempData["success"] = "Đăng ký tài khoản thành công.";
            }
            else
            {
                var userRoles = await _userManager.GetRolesAsync(existingUser);
                if (!userRoles.Contains("User"))
                {
                    await _userManager.AddToRoleAsync(existingUser, "User");
                }

                await _signInManager.SignInAsync(existingUser, isPersistent: false);
            }

            return RedirectToAction("Index", "Home");
        }



    }
}
