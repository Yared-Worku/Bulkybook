using Bulkybookweb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Bulkybookweb.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;

        public AccountController(UserManager<Users> userManager, SignInManager<Users> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Signup model)
        {
            if (ModelState.IsValid)
            {
                bool isFirstUser = !await _userManager.Users.AnyAsync();

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var userObj = new Users
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FName = model.FName,
                    LName = model.LName,
                    PhoneNumber = model.PhoneNumber,
                    Is_Active = true,
                    Created_by = currentUserId != null ? Guid.Parse(currentUserId) : Guid.Empty,
                    Created_date = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(userObj, model.Password);

                if (result.Succeeded)
                {
                    if (isFirstUser)
                    {
                        await _userManager.AddToRoleAsync(userObj, "SuperAdmin");
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(userObj, "Customer");
                    }
                    await _signInManager.SignInAsync(userObj, isPersistent: false);
                    TempData["success"] = "Registration successful! Welcome.";
                    return RedirectToAction("Index", "Category");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Signin model, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Username);

                if (user != null)
                {
                    if (user.Is_Active == false)
                    {
                        ModelState.AddModelError(string.Empty, "Your account is inactive. Please contact the admin.");
                        return View(model);
                    }
                    var result = await _signInManager.PasswordSignInAsync(
                        model.Username,
                        model.Password,
                        model.RememberMe,
                        lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        // If session expired and user was sent here, return them to their previous page
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return LocalRedirect(returnUrl);
                        }
                        return RedirectToAction("Index", "Category");
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}