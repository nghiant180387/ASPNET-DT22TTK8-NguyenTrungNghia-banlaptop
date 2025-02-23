using LaptopStoreShop.Data;
using LaptopStoreShop.Extensions;
using LaptopStoreShop.Models;
using Microsoft.AspNetCore.Mvc;

namespace LaptopStoreShop.Controllers
{
    public class CartController : Controller
    {
        private const string CartSesionKey = "cart";
        public IActionResult GetCartCount()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            int cartCount = cart.Sum(item => item.Quantity);
            return Json(new { count = cartCount });
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
