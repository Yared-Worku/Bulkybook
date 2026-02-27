using Bulkybookweb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Bulkybookweb.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            if (page < 1) page = 1;
            int pageSize = 5;
            var query = _context.Categories.AsNoTracking();
            var totalCategories = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCategories / (double)pageSize);

            if (totalPages < 1) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var categoryList = await _context.Categories
                    .Include(c => c.Creator) 
                    .OrderBy(c => c.DisplayOrder)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(categoryList);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "The display order cannot exactly match name.");
            }

            if (ModelState.IsValid)
            {
                obj.CategoryCode = Guid.NewGuid();
                obj.CreatedDateTime = DateTime.UtcNow;

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    obj.CreatedBy = Guid.Parse(userId);
                }

                _context.Categories.Add(obj);
                _context.SaveChanges();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        [HttpGet]
        public IActionResult Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var category = _context.Categories.Find(id);
            if (category == null) return NotFound();

            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isPrivileged = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

            if (!isPrivileged && category.CreatedBy.ToString() != loggedInUserId)
            {
                TempData["error"] = "Security Warning: You can only edit or delete your own category.";
                return RedirectToAction("Index");
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            // 1. Fetch the original record to verify existence and ownership
            var existingCategory = _context.Categories.AsNoTracking()
                .FirstOrDefault(u => u.CategoryCode == obj.CategoryCode);

            if (existingCategory == null) return NotFound();

            // 2. Security Check (Ownership/Admin)
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isPrivileged = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
            bool isOwner = existingCategory.CreatedBy.ToString() == loggedInUserId;

            if (!isPrivileged && !isOwner)
            {
                TempData["error"] = "Access Denied: You can only edit your own category.";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                // 3. Restore the original audit data so it doesn't get wiped out
                obj.CreatedBy = existingCategory.CreatedBy;
                obj.CreatedDateTime = existingCategory.CreatedDateTime;

                _context.Categories.Update(obj);
                _context.SaveChanges();

                TempData["success"] = "Update successful!";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        [HttpGet]
        public IActionResult Delete(Guid? id) 
        {
            if (id == null || id == Guid.Empty)
                return NotFound();

            var category = _context.Categories.Find(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(Guid? id)
        {
            var category = _context.Categories.Find(id);
            if (category == null) return NotFound();

            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isPrivileged = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

            if (!isPrivileged && category.CreatedBy.ToString() != loggedInUserId)
            {
                TempData["error"] = "Access Denied: You cannot delete this category.";
                return RedirectToAction("Index");
            }

            _context.Categories.Remove(category);
            _context.SaveChanges();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}