using Microsoft.AspNetCore.Mvc;

namespace LaptopStoreShop.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            ViewData["ActivePage"] = "Contact";
            return View();
        }
    }
}
