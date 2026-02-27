using Bulkybookweb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Bulkybookweb.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class RoleController : Controller
    {
        private readonly RoleManager<Roles> _roleManager;
        private readonly UserManager<Users> _userManager; 

        public RoleController(RoleManager<Roles> roleManager, UserManager<Users> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            if (page < 1) page = 1;
            int pageSize = 5;

            // 1. Get the query and total count
            var query = _roleManager.Roles;
            var totalRoles = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRoles / (double)pageSize);

            if (totalPages < 1) totalPages = 1;
            if (page > totalPages) page = totalPages;

            // 2. Fetch the paged roles
            var roles = await query
                .OrderBy(r => r.Name) 
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var roleListWithUsers = new List<RoleIndexViewModel>();

            foreach (var role in roles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);

                roleListWithUsers.Add(new RoleIndexViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    UserNames = usersInRole.Select(u => u.FName + " " + u.LName).ToList()
                });
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(roleListWithUsers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_CreatePartial", new Roles());

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Roles role)
        {
            if (string.IsNullOrWhiteSpace(role.Name))
            {
                ModelState.AddModelError("Name", "Role Name is required.");
                return View(role);
            }

            if (ModelState.IsValid)
            {
                role.Id = Guid.NewGuid();
                role.Name = role.Name.Trim();
                role.NormalizedName = role.Name.ToUpper();
                role.Created_date = DateTime.Now;

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(currentUserId))
                    role.Created_by = Guid.Parse(currentUserId);

                var result = await _roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    TempData["success"] = "Role created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            return View(role);
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles(Guid id) 
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null) return NotFound();

            var allUsers = await _userManager.Users.ToListAsync();

            var model = new RoleUserAssignmentViewModel
            {
                RoleId = id,
                RoleName = role.Name,
                Users = new List<UserSelection>()
            };

            foreach (var user in allUsers)
            {
                model.Users.Add(new UserSelection
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    IsSelected = await _userManager.IsInRoleAsync(user, role.Name)
                });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(RoleUserAssignmentViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.RoleId.ToString());
            if (role == null) return NotFound();

            foreach (var userItem in model.Users)
            {
                var user = await _userManager.FindByIdAsync(userItem.UserId.ToString());
                if (user != null)
                {
                    if (userItem.IsSelected && !(await _userManager.IsInRoleAsync(user, role.Name)))
                    {
                        await _userManager.AddToRoleAsync(user, role.Name);
                    }
                    else if (!userItem.IsSelected && await _userManager.IsInRoleAsync(user, role.Name))
                    {
                        await _userManager.RemoveFromRoleAsync(user, role.Name);
                    }
                }
            }

            TempData["success"] = "Users updated for role " + role.Name;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty) return NotFound();

            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null) return NotFound();

            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Roles role)
        {
            if (ModelState.IsValid)
            {
                var roleFromDb = await _roleManager.FindByIdAsync(role.Id.ToString());
                if (roleFromDb == null) return NotFound();

                roleFromDb.Name = role.Name.Trim();
                roleFromDb.NormalizedName = role.Name.Trim().ToUpper();

                var result = await _roleManager.UpdateAsync(roleFromDb);

                if (result.Succeeded)
                {
                    TempData["success"] = "Role updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            return View(role);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null) return Json(new { success = false, message = "Role not found." });

            if (role.Name == "SuperAdmin")
            {
                return Json(new { success = false, message = "System roles cannot be deleted." });
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            if (usersInRole.Any())
            {
                return Json(new { success = false, message = $"Cannot delete. {usersInRole.Count} users are still assigned to this role." });
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = "Deleted successfully" });
            }

            return Json(new { success = false, message = "Error occurred while deleting." });
        }
    }
}