//using Bulkybookweb.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Data.SqlClient;
//using System.Data;

//namespace Bulkybookweb.Controllers
//{
//    public class CategoryController : Controller
//    {
//        private readonly IConfiguration _config;

//        public CategoryController(IConfiguration config)
//        {
//            _config = config;
//        }

//        public IActionResult Index()
//        {
//            string connStr = _config.GetConnectionString("BULKY_DB");

//            var categories = new List<Category>();

//            using SqlConnection conn = new SqlConnection(connStr);
//            using SqlCommand cmd = new SqlCommand("proc_GetCategories", conn)
//            {
//                CommandType = CommandType.StoredProcedure
//            };

//            try
//            {
//                conn.Open();
//                using SqlDataReader reader = cmd.ExecuteReader();
//                while (reader.Read())
//                {
//                    categories.Add(new Category
//                    {
//                        Id = (int)reader["Id"],
//                        Name = reader["Name"].ToString(),
//                        DisplayOrder = (int)reader["DisplayOrder"],
//                        CreatedDateTime = (DateTime)reader["CreatedDateTime"]
//                    });
//                }

//                return View(categories);
//            }
//            catch (Exception ex)
//            {
//                return Content("Error: " + ex.Message);
//            }
//        }

//        [HttpGet]
//        public IActionResult Create()
//        {
//            return View();
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult Create(Category obj)
//        {
//            if(obj.Name == obj.DisplayOrder.ToString())
//            {
//                ModelState.AddModelError("Name", "The display order cannot exactly match name.");
//            }
//            if (ModelState.IsValid)
//            {
//                string connStr = _config.GetConnectionString("BULKY_DB");

//                try
//                {
//                    using SqlConnection conn = new SqlConnection(connStr);
//                    using SqlCommand cmd = new SqlCommand("proc_InsertCategory", conn)
//                    {
//                        CommandType = CommandType.StoredProcedure
//                    };

//                    cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = obj.Name;
//                    cmd.Parameters.Add("@DisplayOrder", SqlDbType.Int).Value = obj.DisplayOrder;
//                    cmd.Parameters.Add("@CreatedDateTime", SqlDbType.DateTime).Value = obj.CreatedDateTime;

//                    conn.Open();
//                    cmd.ExecuteNonQuery();
//                    TempData["success"] = "category created successfully";
//                    return RedirectToAction("Index");
//                }
//                catch (SqlException ex)
//                {
//                    TempData["error"] = "Faild to create category!";
//                    return Content("Database Error: " + ex.Message);
//                }
//            }

//            return View(obj);
//        }
//        [HttpGet]
//        public IActionResult Edit(int? id)
//        {
//            if (id == null || id == 0)
//                return NotFound();

//            string connStr = _config.GetConnectionString("BULKY_DB");
//            Category category = null;

//            try
//            {
//                using SqlConnection conn = new SqlConnection(connStr);
//                using SqlCommand cmd = new SqlCommand("proc_GetCategoryById", conn)
//                {
//                    CommandType = CommandType.StoredProcedure
//                };

//                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

//                conn.Open();
//                using SqlDataReader reader = cmd.ExecuteReader();
//                if (reader.Read())
//                {
//                    category = new Category
//                    {
//                        Id = (int)reader["Id"],
//                        Name = reader["Name"].ToString(),
//                        DisplayOrder = (int)reader["DisplayOrder"],
//                        CreatedDateTime = (DateTime)reader["CreatedDateTime"]
//                    };
//                }
//            }
//            catch (SqlException ex)
//            {
//                return Content("Database Error: " + ex.Message);
//            }

//            if (category == null)
//                return NotFound();

//            return View(category);
//        }
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult Edit(Category obj)
//        {
//            if (obj.Name == obj.DisplayOrder.ToString())
//            {
//                ModelState.AddModelError("Name", "The display order cannot exactly match name.");
//            }

//            if (ModelState.IsValid)
//            {
//                string connStr = _config.GetConnectionString("BULKY_DB");

//                try
//                {
//                    using SqlConnection conn = new SqlConnection(connStr);
//                    using SqlCommand cmd = new SqlCommand("proc_UpdateCategory", conn)
//                    {
//                        CommandType = CommandType.StoredProcedure
//                    };

//                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = obj.Id;
//                    cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = obj.Name;
//                    cmd.Parameters.Add("@DisplayOrder", SqlDbType.Int).Value = obj.DisplayOrder;

//                    conn.Open();
//                    cmd.ExecuteNonQuery();
//                    TempData["success"] = "category updated successfully";
//                    return RedirectToAction("Index");
//                }
//                catch (SqlException ex)
//                {
//                    TempData["error"] = "Faild to update category!";
//                    return Content("Database Error: " + ex.Message);
//                }
//            }

//            return View(obj);
//        }
//        [HttpGet]
//        public IActionResult Delete(int? id)
//        {
//            if (id == null || id == 0)
//                return NotFound();

//            string connStr = _config.GetConnectionString("BULKY_DB");
//            Category category = null;

//            try
//            {
//                using SqlConnection conn = new SqlConnection(connStr);
//                using SqlCommand cmd = new SqlCommand("proc_GetCategoryById", conn)
//                {
//                    CommandType = CommandType.StoredProcedure
//                };

//                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

//                conn.Open();
//                using SqlDataReader reader = cmd.ExecuteReader();
//                if (reader.Read())
//                {
//                    category = new Category
//                    {
//                        Id = (int)reader["Id"],
//                        Name = reader["Name"].ToString(),
//                        DisplayOrder = (int)reader["DisplayOrder"],
//                        CreatedDateTime = (DateTime)reader["CreatedDateTime"]
//                    };
//                }
//            }
//            catch (SqlException ex)
//            {
//                return Content("Database Error: " + ex.Message);
//            }

//            if (category == null)
//                return NotFound();

//            return View(category);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult Delete(Category obj)
//        {
//            string connStr = _config.GetConnectionString("BULKY_DB");

//            try
//            {
//                using SqlConnection conn = new SqlConnection(connStr);
//                using SqlCommand cmd = new SqlCommand("proc_DeleteCategory", conn)
//                {
//                    CommandType = CommandType.StoredProcedure
//                };

//                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = obj.Id;

//                conn.Open();
//                cmd.ExecuteNonQuery();
//                TempData["success"] = "category deleted successfully";
//                return RedirectToAction("Index");
//            }
//            catch (SqlException ex)
//            {
//                TempData["error"] = "Faild to delete category!";
//                return Content("Database Error: " + ex.Message);
//            }
//        }
//    }
//}

using Bulkybookweb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bulkybookweb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var categories = _context.Categories.OrderBy(c => c.DisplayOrder).ToList();
            return View(categories);
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
                obj.CreatedDateTime = DateTime.UtcNow;
                _context.Categories.Add(obj);
                _context.SaveChanges();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var category = _context.Categories.Find(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "The display order cannot exactly match name.");
            }

            if (ModelState.IsValid)
            {
                _context.Categories.Update(obj);
                _context.SaveChanges();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }

            return View(obj);
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var category = _context.Categories.Find(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Category obj)
        {
            var category = _context.Categories.Find(obj.Id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
                TempData["success"] = "Category deleted successfully";
            }
            else
            {
                TempData["error"] = "Category not found!";
            }
            return RedirectToAction("Index");
        }
    }
}