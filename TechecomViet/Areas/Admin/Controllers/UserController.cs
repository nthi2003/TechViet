using TechecomViet.Models.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechecomViet.Models;
using TechecomViet.Reponsitory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TechecomViet.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Route("Admin/User")]
    [Authorize(Roles = "ADMIN")]
    public class UserController : Controller
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _dataContext;



        public UserController(DataContext context,
            UserManager<UserModel> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {

            _userManager = userManager;
            _roleManager = roleManager;
            _dataContext = context;

        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usersWithRoles = await (from u in _dataContext.Users
                                        join ur in _dataContext.UserRoles on u.Id equals ur.UserId
                                        join r in _dataContext.Roles on ur.RoleId equals r.Id
                                        select new
                                        {
                                            User = new
                                            {
                                                u.Id,
                                                u.UserName,
                                                u.Email,
                                                u.FullName,
                                                u.PhoneNumber,
                                                u.Status
                                            },
                                            RoleName = r.Name,
                                            RoleId = r.Id
                                        })
                                 .ToListAsync();

            var usersViewModel = usersWithRoles.Select(u => new UserWithRoleViewModel
            {
                Id = u.User.Id,
                UserName = u.User.UserName,
                Email = u.User.Email,
                FullName = u.User.FullName,
                PhoneNumber = u.User.PhoneNumber,
                RoleName = u.RoleName,
                RoleId = u.RoleId,
                Status = u.User.Status
            }).ToList();
            ViewBag.Roles = new SelectList(await _dataContext.Roles.ToListAsync(), "Id", "Name");
            return View(usersViewModel);
        }
        [Route("Create")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = new SelectList(await _roleManager.Roles.ToListAsync(), "Id", "Name");
            return View();
        }
        [Route("Create")]
        [HttpPost]
        public async Task<IActionResult> Create(UserModel user, string RoleId)
        {
            var checkUser = await _userManager.FindByEmailAsync(user.Email);
            if (checkUser != null)
            {
                TempData["error"] = "Người dùng đã tồn tại";
                return View(user);
            }
            var createUserResult = await _userManager.CreateAsync(user, user.PasswordHash);
            if (!createUserResult.Succeeded)
            {
                TempData["error"] = "Lỗi khi tạo người dùng: ";
                return View(user);
            }
            var newUser = await _userManager.FindByEmailAsync(user.Email);
            if (newUser == null)
            {
                TempData["error"] = "Không tìm thấy người dùng sau khi tạo.";
                return View(user);
            }

            var role = await _roleManager.FindByIdAsync(RoleId);
            if (role != null)
            {
                await _userManager.AddToRoleAsync(newUser, role.Name);
            }

            TempData["success"] = "Thêm người dùng thành công";
            return RedirectToAction("Index");
        }


        [Route("SaveUser")]
        [HttpPost]
        public async Task<IActionResult> SaveUser(UserWithRoleViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                TempData["error"] = "Không tìm thấy người dùng";
                return RedirectToAction("Index");
            }
            user.Status = model.Status;
            var newRole = await _roleManager.FindByIdAsync(model.RoleId);
            if (newRole == null)
            {
                TempData["error"] = "Vai trò không hợp lệ";
                return RedirectToAction("Index");
            }
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Count > 0 && !currentRoles.Contains(newRole.Name))
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, newRole.Name);
            }
            _dataContext.Update(user);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Cập nhật thành công!";
            return RedirectToAction("Index");
        }

    }
}


