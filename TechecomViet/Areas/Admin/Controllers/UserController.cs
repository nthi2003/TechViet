using TechecomViet.Models.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechecomViet.Models;
using TechecomViet.Reponsitory;
using Microsoft.AspNetCore.Authorization;

namespace TechecomViet.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Route("Admin/User")]
    [Authorize(Roles = "Admin")]
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
                                                u.PhoneNumber
                                            },
                                            RoleName = r.Name
                                        })
                                 .ToListAsync();

            var usersViewModel = usersWithRoles.Select(u => new UserWithRoleViewModel
            {
                Id = u.User.Id,
                UserName = u.User.UserName,
                Email = u.User.Email,
                FullName = u.User.FullName,
                PhoneNumber = u.User.PhoneNumber,
                RoleName = u.RoleName
            }).ToList();

            return View(usersViewModel);
        }


    }
}
