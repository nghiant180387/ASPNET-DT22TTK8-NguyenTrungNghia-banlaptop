using LaptopStoreShop.Data;
using LaptopStoreShop.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LaptopStoreShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/product")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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
            var laptops = _context.Laptops
                .Include(s => s.Category)
                .Include(s => s.Brand)
                .Include(s => s.LaptopImages)
                .ToList();
            return View(laptops);
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
            ViewBag.Category = new SelectList(_context.Categories, "Id", "CategoryName");
            ViewBag.Brands = new SelectList(_context.Brands, "Id", "BrandName");
            return View();
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(Laptop laptop, List<IFormFile> images)
        {
            var role = HttpContext.Session.GetString("UserRole");
            Console.WriteLine(role);

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            ViewBag.Category = new SelectList(_context.Categories, "Id", "CategoryName");
            ViewBag.Brands = new SelectList(_context.Brands, "Id", "BrandName");

            _context.Laptops.Add(laptop);
            await _context.SaveChangesAsync();

            if (images != null && images.Count > 0)
            {
                foreach (var image in images)
                {
                    if (image.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                        var filePath = Path.Combine("wwwroot/uploads", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        var laptopImage = new LaptopImage
                        {
                            LaptopId = laptop.Id,
                            ImageUrl = "/uploads/" + fileName
                        };

                        _context.LaptopImages.Add(laptopImage);
                    }
                }
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Thêm sản phẩm thành công!";
            return RedirectToAction("Index");
        }

        [HttpGet("edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            Console.WriteLine(role);

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            var laptop = await _context.Laptops
                .Include(l => l.LaptopImages) 
                .FirstOrDefaultAsync(l => l.Id == id);

            if (laptop == null)
            {
                return NotFound();
            }

            if (laptop.LaptopImages == null)
            {
                laptop.LaptopImages = new List<LaptopImage>();
            }

            return View(laptop);
        }

        [HttpPost("edit")]
        public async Task<IActionResult> Edit (int id, Laptop laptop, List<IFormFile> images)
        {
            var role = HttpContext.Session.GetString("UserRole");
            Console.WriteLine(role);

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            var existingLaptop = _context.Laptops
              .Include(s => s.LaptopImages)
              .FirstOrDefault(s => s.Id == id);

            if (existingLaptop == null)
                return NotFound();

            existingLaptop.Name = laptop.Name;
            existingLaptop.Description = laptop.Description;
            existingLaptop.Price = laptop.Price;
            existingLaptop.CategoryId = laptop.CategoryId;
            existingLaptop.BrandId = laptop.BrandId;
            existingLaptop.UpdatedAt = DateTime.Now;

            //if (laptop.LaptopImages.Any())
            //{
            //    foreach (var oldImage in laptop.LaptopImages)
            //    {
            //        string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", oldImage.ImageUrl);
            //        if (System.IO.File.Exists(oldImagePath))
            //        {
            //            System.IO.File.Delete(oldImagePath);
            //        }
            //    }
            //    _context.LaptopImages.RemoveRange(laptop.LaptopImages);
            //}

            //var oldImages = _context.LaptopImages.Where(img => img.LaptopId == id).ToList();
            //_context.LaptopImages.RemoveRange(oldImages);
            //await _context.SaveChangesAsync(); 

            if (images != null && images.Count > 0)
            {
                foreach (var image in images)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    var newImage = new LaptopImage
                    {
                        LaptopId = id,
                        ImageUrl = "/uploads/" + fileName,
                        CreatedAt = DateTime.Now
                    };

                    _context.LaptopImages.Add(newImage);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction("Index");
        }
        [HttpGet("delete")]
        public IActionResult Delete(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            Console.WriteLine(role);

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            var laptop = _context.Laptops
                .Include(s => s.LaptopImages)
                .FirstOrDefault(s => s.Id == id);
            if (laptop == null)
                return NotFound();

            _context.Laptops.Remove(laptop);
            _context.SaveChanges();

            TempData["Success"] = "Xóa sản phẩm thành công!";
            return RedirectToAction("Index");
        }
    }
}
