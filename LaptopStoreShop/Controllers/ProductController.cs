using LaptopStoreShop.Data;
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
        public IActionResult Index()
        {
            return View();
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
