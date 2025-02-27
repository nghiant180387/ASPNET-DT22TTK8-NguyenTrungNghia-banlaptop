using LaptopStoreShop.Data;
using LaptopStoreShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaptopStoreShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/brand")]
    public class BrandController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BrandController(ApplicationDbContext context)
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
            var brands = _context.Brands.ToList();
            return View(brands);
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
        public async Task<IActionResult> Create(Brand brand)
        {
            var role = HttpContext.Session.GetString("UserRole");
            Console.WriteLine(role);

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            _context.Add(brand);
            await _context.SaveChangesAsync();
            if (brand.BrandName == null)
            {
                TempData["error"] = "Vui lòng nhập đầy đủ thông tin";
                return View(brand);
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
            var brands = _context.Brands.Find(id);
            return View(brands);
        }

        [HttpPost("edit/{id}")]
        public IActionResult Edit(Brand model)
        {
            var role = HttpContext.Session.GetString("UserRole");
            Console.WriteLine(role);

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            var brand = _context.Brands.Find(model.Id);
            if (brand == null)
            {
                return NotFound();
            }

            brand.BrandName = model.BrandName;
            brand.Status = model.Status;

            _context.Update(brand);
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
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            bool hasRelatedProducts = await _context.Laptops.AnyAsync(p => p.BrandId == id);

            if (hasRelatedProducts)
            {
                TempData["Error"] = "Không thể xóa danh mục vì còn sản phẩm liên quan.";
                return RedirectToAction(nameof(Index));
            }
            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa danh mục thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
