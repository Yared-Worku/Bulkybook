using Bulkybookweb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Bulkybookweb.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<Roles> _roleManager;

        public UserManagementController(UserManager<Users> userManager, RoleManager<Roles> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            if (page < 1) page = 1;
            int pageSize = 5;
            var query = _userManager.Users.AsNoTracking();
            var totalUsers = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var userList = await query
                .OrderByDescending(u => u.Created_date)
                .ThenBy(u => u.Id) 
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(userList);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string fName, string lName, string userName, string email, string phoneNumber, string password)
        {
            if (ModelState.IsValid)
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = new Users
                {
                     FName = fName, 
                     LName = lName,
                    UserName = userName,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    Is_Active = true,
                    Created_date = DateTime.UtcNow,
                    Created_by = currentUserId != null ? Guid.Parse(currentUserId) : Guid.Empty,
                };

                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    TempData["success"] = "User created successfully.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Activation(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Activation")] 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivationConfirmed(Guid id) 
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();
            user.Is_Active = !(user.Is_Active ?? false);
            // Safety: Always refresh the security stamp when changing status
            await _userManager.UpdateSecurityStampAsync(user);
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["success"] = user.Is_Active == true ? "User Activated!" : "User Deactivated!";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty) return NotFound();

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Users model)
        {
            if (ModelState.IsValid)
            {
                var userFromDb = await _userManager.FindByIdAsync(model.Id.ToString());
                if (userFromDb == null) return NotFound();

                userFromDb.FName = model.FName;
                userFromDb.LName = model.LName;
                userFromDb.Email = model.Email;
                userFromDb.PhoneNumber = model.PhoneNumber;

                var result = await _userManager.UpdateAsync(userFromDb);

                if (result.Succeeded)
                {
                    TempData["success"] = "User updated successfully.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }
    }
}