using Microsoft.AspNetCore.Mvc;

namespace LaptopStoreShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/order")]
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Create()
        {
            return View();
        }
    }
}
