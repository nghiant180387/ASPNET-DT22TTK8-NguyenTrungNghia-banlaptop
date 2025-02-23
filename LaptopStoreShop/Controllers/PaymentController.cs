using LaptopStoreShop.Extensions;
using LaptopStoreShop.Models;
using Microsoft.AspNetCore.Mvc;

namespace LaptopStoreShop.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Index()
        {
            var cartItems = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart") ?? new List<CartItem>();

            return View(cartItems);
        }
        [HttpPost]
        public IActionResult AddToCart(int id, string name, decimal price, string imageUrl, int qty, int stock)
        {
            if (qty > stock)
            {
                return Json(new { success = false, message = "Số lượng sản phẩm trong kho không đủ" });
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");

            if (cart == null)
            {
                cart = new List<CartItem>();
            }
            var item = cart.FirstOrDefault(c => c.Id == id);

            if (item == null)
            {
                cart.Add(new CartItem
                {
                    Id = id,
                    Name = name,
                    Price = price,
                    ImageUrl = imageUrl,
                    Quantity = qty
                });
            }
            else
            {
                item.Quantity++;
            }

            HttpContext.Session.SetObjectAsJson("cart", cart);

            return RedirectToAction("Index");
        }
    }
}
