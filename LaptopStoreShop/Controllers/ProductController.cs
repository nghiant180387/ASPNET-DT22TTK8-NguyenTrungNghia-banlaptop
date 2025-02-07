using Microsoft.AspNetCore.Mvc;

namespace LaptopStoreShop.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
