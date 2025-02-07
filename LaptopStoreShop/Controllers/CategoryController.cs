using Microsoft.AspNetCore.Mvc;

namespace LaptopStoreShop.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
