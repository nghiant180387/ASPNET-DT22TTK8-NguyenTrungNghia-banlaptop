using System.Drawing;
using LaptopStoreShop.Data;
using LaptopStoreShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaptopStoreShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index(int? brand, int? category, string keyword, string sort = "", int page = 1, int pageSize = 6)
        {
            ViewData["ActivePage"] = "Product";
            var categories = _context.Categories
                .Where(c => c.Status == 1)
                 .Select(c => new
                 {
                     c.Id,
                     c.CategoryName,
                     ProductCount = _context.Laptops.Count(p => p.CategoryId == c.Id)
                 })
                .ToList();

            var brands = _context.Brands
                .Where(b => b.Status == 1)
                .Select(c => new
                {
                    c.Id,
                    c.BrandName,
                    ProductCount = _context.Laptops.Count(p => p.BrandId == c.Id)
                })
                .ToList();

            ViewBag.Category = categories;
            ViewBag.Brand = brands;

            var laptops = _context.Laptops
                .Include(l => l.Category)
                .Include(l => l.Brand)
                .Include(l => l.LaptopImages)
                .Where(l => l.Status == 1)
                .AsQueryable();
            if (category.HasValue)
            {
                laptops = laptops.Where(p => p.CategoryId == category.Value);
            }

            if (brand.HasValue)
            {
                laptops = laptops.Where(p => p.BrandId == brand.Value);
            }


            if (!string.IsNullOrEmpty(keyword))
            {
                laptops = laptops.Where(p => p.Name.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(sort))
            {
                if (sort == "price-asc")
                {
                    laptops = laptops.OrderBy(p => p.Price);
                }
                else if (sort == "price-desc")
                {
                    laptops = laptops.OrderByDescending(p => p.Price);
                }
            }


            int totalItems = laptops.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            laptops = laptops.Skip((page - 1) * pageSize).Take(pageSize);
            Console.WriteLine("check page: " + page);
            Console.WriteLine("check total page: " + totalPages);
            Console.WriteLine("check total item: " + totalItems);
            ViewBag.SelectedBrand = brand;
            ViewBag.SelectedCategory = category;
            ViewBag.Sort = sort;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            return View(laptops.ToList());
        }
        public IActionResult Detail(int id)
        {

            var laptop = _context.Laptops
                .Include(l => l.Category)
                .Include(l => l.Brand)
                .Include(l => l.LaptopImages)
                .FirstOrDefault(l => l.Id == id);
            return View(laptop);
        }
    }
}
