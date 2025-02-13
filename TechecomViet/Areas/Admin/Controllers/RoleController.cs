﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechecomViet.Reponsitory;

namespace TechecomViet.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class RoleController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        public RoleController(DataContext context, RoleManager<IdentityRole> roleManager)
        {
            _dataContext = context;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Roles.OrderByDescending(p => p.Id).ToListAsync());
        }
        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound(); 
            }
            var role = await _roleManager.FindByIdAsync(id);

            return View(role);
        }

        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, IdentityRole model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound(); 
            }
            if (ModelState.IsValid) 
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound();
                }
                role.Name = model.Name;
                try
                {
                    await _roleManager.UpdateAsync(role);
                    TempData["success"] = "Role updated successfully!";
                    return RedirectToAction("Index"); 
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "An error occurred while updating the role.");
                }

            }
            return View(model ?? new IdentityRole { Id = id });
        }

        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(IdentityRole model)
        {
            if (!_roleManager.RoleExistsAsync(model.Name).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(model.Name)).GetAwaiter().GetResult();
            }
            return Redirect("Index");
        }

        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound(); 
            }

            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound(); 
            }

            try
            {
                await _roleManager.DeleteAsync(role);
                TempData["success"] = "Quyền đã được xóa";
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while deleting the role.");
            }

            return Redirect("Index");
        }

    }
}
