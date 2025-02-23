using System.Diagnostics;
using LaptopStoreShop.Data;
using LaptopStoreShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaptopStoreShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.Laptops
                .Include(l => l.Category)
                .Include(l => l.LaptopImages)
                .Where(l => l.Status == 1)
                .OrderBy(x => Guid.NewGuid())
                .ToList();
            return View(products);
        }
    }
}
