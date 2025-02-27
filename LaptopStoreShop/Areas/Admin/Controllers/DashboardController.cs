using Microsoft.AspNetCore.Mvc;

namespace LaptopStoreShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            Console.WriteLine(role);

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
    }
}
