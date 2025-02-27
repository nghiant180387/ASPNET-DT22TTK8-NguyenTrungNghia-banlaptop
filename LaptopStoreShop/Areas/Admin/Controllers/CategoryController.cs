using LaptopStoreShop.Data;
using LaptopStoreShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaptopStoreShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/category")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            Console.WriteLine(role);

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            var categories = _context.Categories.ToList();
            return View(categories);
        }
        [HttpGet("create")]
        public IActionResult Create()
        {
            var role = HttpContext.Session.GetString("UserRole");
            Console.WriteLine(role);

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create(Category category)
        {
            var role = HttpContext.Session.GetString("UserRole");
            Console.WriteLine(role);

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            _context.Add(category);
            await _context.SaveChangesAsync();
            if (category.CategoryName == null)
            {
                TempData["error"] = "Vui lòng nhập đầy đủ thông tin";
                return View(category);
            }
            TempData["success"] = "Thêm danh mục thành công";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("edit/{id}")]
        public IActionResult Edit(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            Console.WriteLine(role);

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            var category = _context.Categories.Find(id);
            return View(category);
        }

        [HttpPost("edit/{id}")]
        public IActionResult Edit(Category model)
        {
            var role = HttpContext.Session.GetString("UserRole");
            Console.WriteLine(role);

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            var category = _context.Categories.Find(model.Id);
                if (category == null)
                {
                    return NotFound();
                }

                category.CategoryName = model.CategoryName;
                category.Status = model.Status;

                _context.Update(category);
                _context.SaveChanges();

                TempData["Success"] = "Cập nhật danh mục thành công!";
                return RedirectToAction("Index");
        }

        [Route("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            Console.WriteLine(role);

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            bool hasRelatedProducts = await _context.Laptops.AnyAsync(p => p.CategoryId == id);

            if (hasRelatedProducts)
            {
                TempData["Error"] = "Không thể xóa danh mục vì còn sản phẩm liên quan.";
                return RedirectToAction(nameof(Index));
            }
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa danh mục thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
