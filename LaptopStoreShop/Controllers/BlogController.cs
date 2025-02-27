using Microsoft.AspNetCore.Mvc;

namespace LaptopStoreShop.Controllers
{
    public class BlogController : Controller
    {
        public IActionResult Index()
        {
            ViewData["ActivePage"] = "Blog";
            return View();
        }
    }
}
