using Microsoft.AspNetCore.Mvc;

namespace LaptopStoreShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
