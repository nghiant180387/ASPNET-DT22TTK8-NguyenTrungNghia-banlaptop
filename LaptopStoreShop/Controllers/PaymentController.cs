using LaptopStoreShop.Data;
using LaptopStoreShop.Extensions;
using LaptopStoreShop.Models;
using LaptopStoreShop.Models.Vnpay;
using LaptopStoreShop.Services.Momo;
using LaptopStoreShop.Services.Vnpay;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaptopStoreShop.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMomoService _momoService;
        private readonly IVnPayService _vnPayService;
        public PaymentController(ApplicationDbContext dbContext, IMomoService momoService, IVnPayService vnPayService)
        {
            _dbContext = dbContext;
            _momoService = momoService;
            _vnPayService = vnPayService;
        }
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
                TempData["Error"]= "Số lượng sản phẩm trong kho không đủ";
                return RedirectToAction("Detail", "Product", new { id = id });
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
            TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng";
            return RedirectToAction("Detail", "Product", new { id = id });
        }
        [HttpPost]
        //RemoveFromCart
        public IActionResult RemoveFromCart(int id)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");

            if (cart == null)
            {
                cart = new List<CartItem>();
            }

            var itemToRemove = cart.FirstOrDefault(c => c.Id == id);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
            }

            HttpContext.Session.SetObjectAsJson("cart", cart);

            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public async Task<IActionResult> Checkout(OrderInfo model, PaymentInformationModel model1, string FullName, string address, string phone, decimal Amount, string paymentMethod)
        {

            Console.WriteLine("Name" + FullName);
            Console.WriteLine("Address" + address);
            Console.WriteLine("Phone" + phone);
            Console.WriteLine("Amount" + Amount);
            Console.WriteLine("PaymentMethod" + paymentMethod);

            HttpContext.Session.SetString("CustomerName", FullName);
            HttpContext.Session.SetString("Address", address);
            HttpContext.Session.SetString("Phone", phone);
            HttpContext.Session.SetDecimal("TotalAmount", Amount);

            if (string.IsNullOrEmpty(paymentMethod))
            {
                TempData["Error"] = "Vui lòng chọn phương thức thanh toán.";
                return RedirectToAction("Index", "Cart");
            }
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }
            var productIds = cart.Select(c => c.Id).ToList();
            var products = _dbContext.Laptops.Where(p => productIds.Contains(p.Id)).ToList();

            foreach (var cartItem in cart)
            {
                var product = products.FirstOrDefault(p => p.Id == cartItem.Id);
                if (product != null && cartItem.Quantity > product.Stock)
                {
                    TempData["Error"] = $"Sản phẩm {product.Name} chỉ còn {product.Stock} sản phẩm trong kho.";
                    return RedirectToAction("Index", "Cart");
                }
            }
            switch (paymentMethod)
            {
                case "momo":
                    var response = await _momoService.CreatePaymentAsync(model);
                    return Redirect(response.PayUrl);

                case "vnpay":
                    var url = _vnPayService.CreatePaymentUrl(model1, HttpContext);
                    return Redirect(url);

                case "cod":
                    var order = new Order
                    {
                        OrderId = DateTime.Now.ToString("yyyyMMddHHmmss"),
                        CustomerName = FullName,
                        Address = address,
                        Phone = phone,
                        TotalAmount = Amount,
                        PaymentMethod = "COD",
                        PaymentStatus = "Success",
                        OrderStatus = "Processing",
                        OrderDate = DateTime.Now
                    };

                    _dbContext.Orders.Add(order);
                    await _dbContext.SaveChangesAsync();

                    if (cart == null || !cart.Any())
                    {
                        throw new ArgumentException("Cart không hợp lệ!");
                    }

                    var orderDetails = cart.Select(item => new OrderDetail
                    {
                        OrderId = order.OrderId,
                        ProductId = item.Id,
                        ProductName = item.Name,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        Total = item.Quantity * item.Price
                    }).ToList();


                    HttpContext.Session.Remove("cart");

                    ViewBag.Message = "Thanh toán thành công. Cảm ơn bạn đã mua hàng tại Apple Store.";
                    TempData["PaymentSuccess"] = "Thanh toán thành công!";
                    return RedirectToAction("Index", "Home");

                default:
                    TempData["Error"] = "Phương thức thanh toán không hợp lệ.";
                    return RedirectToAction("Index", "Cart");
            }

        }

        private List<OrderDetail> SaveOrderDetails(List<CartItem> cart, string orderId)
        {
            if (cart == null || !cart.Any())
            {
                throw new ArgumentException("Cart không hợp lệ!");
            }

            var orderDetails = cart.Select(item => new OrderDetail
            {
                OrderId = orderId,
                ProductId = item.Id,
                ProductName = item.Name,
                Quantity = item.Quantity,
                Price = item.Price,
                Total = item.Quantity * item.Price
            }).ToList();


            _dbContext.OrderDetails.AddRange(orderDetails);
            _dbContext.SaveChanges();

            HttpContext.Session.Remove("cart");
            TempData["Success"] = "Thanh toán thành công. Cảm ơn bạn đã mua hàng tại Shoe Store!";
            return orderDetails;
        }
        [Route("Payment/PaymentCallbackVnpay")]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            var CustomerName = HttpContext.Session.GetString("CustomerName");
            var Address = HttpContext.Session.GetString("Address");
            var Phone = HttpContext.Session.GetString("Phone");
            var TotalAmount = HttpContext.Session.GetDecimal("TotalAmount");
            if (response.VnPayResponseCode == "00")
            {

                var checkOrder = new Order
                {
                    CustomerName = CustomerName,
                    Address = Address,
                    Phone = Phone,
                    TotalAmount = TotalAmount,
                    PaymentMethod = response.PaymentMethod,
                    OrderId = response.OrderId,
                    PaymentStatus = "Success",
                    OrderStatus = "Processing",
                    OrderDate = DateTime.Now
                };

                _dbContext.Orders.Add(checkOrder);
                await _dbContext.SaveChangesAsync();
                var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");

                if (cart == null || !cart.Any())
                {
                    ViewBag.Message = "Giỏ hàng của bạn đang trống.";
                    return RedirectToAction("Index", "Home");
                }

                SaveOrderDetails(cart, checkOrder.OrderId);

                foreach (var item in cart)
                {
                    var laptop = _dbContext.Laptops.FirstOrDefault(p => p.Id == item.Id);
                    if (laptop != null && laptop.Stock >= item.Quantity)
                    {
                        laptop.Stock -= item.Quantity;
                    }
                }

                _dbContext.SaveChanges();
                TempData["Success"] = "Thanh toán thành công. Cảm ơn bạn đã mua hàng tại Shoe Store!";
            }
            else
            {
                var checkOrder = new Order
                {
                    CustomerName = CustomerName,
                    Address = Address,
                    Phone = Phone,
                    TotalAmount = TotalAmount,
                    PaymentMethod = response.PaymentMethod,
                    OrderId = response.OrderId,
                    PaymentStatus = "Fail",
                    OrderStatus = "Processing",
                    OrderDate = DateTime.Now
                };

                _dbContext.Orders.Add(checkOrder);
                await _dbContext.SaveChangesAsync();
                var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");

                if (cart == null || !cart.Any())
                {
                    ViewBag.Message = "Giỏ hàng của bạn đang trống.";
                    return RedirectToAction("Index", "Home");
                }

                SaveOrderDetails(cart, checkOrder.OrderId);
                ViewBag.Message = "Thanh toán thất bại. Vui lòng thử lại hoặc liên hệ hỗ trợ.";
            }

            return View("PaymentCallbackVnpay", response);

        }

        [HttpGet]
        [Route("Payment/PaymentCallBack")]
        public async Task<IActionResult> PaymentCallBack()
        {
            var response = await _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            var requestQuery = HttpContext.Request.Query;

            var CustomerName = HttpContext.Session.GetString("CustomerName");
            var Address = HttpContext.Session.GetString("Address");
            var Phone = HttpContext.Session.GetString("Phone");
            var TotalAmount = HttpContext.Session.GetDecimal("TotalAmount");

            if (response.IsSuccess)
            {
                var checkOrder = new Order
                {
                    CustomerName = CustomerName,
                    Address = Address,
                    Phone = Phone,
                    TotalAmount = TotalAmount,
                    PaymentMethod = "Momo",
                    OrderId = requestQuery["orderId"],
                    PaymentStatus = "Success",
                    OrderStatus = "Procesing",
                    OrderDate = DateTime.Now
                };

                _dbContext.Orders.Add(checkOrder);
                await _dbContext.SaveChangesAsync();
                var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");

                if (cart == null || !cart.Any())
                {
                    ViewBag.Message = "Giỏ hàng của bạn đang trống.";
                    return RedirectToAction("Index", "Home");
                }

                SaveOrderDetails(cart, checkOrder.OrderId);
                ViewBag.Message = "Thanh toán thành công. Cảm ơn bạn đã mua hàng tại Shoe Store.";
                TempData["Success"] = "Thanh toán thành công. Cảm ơn bạn đã mua hàng tại Shoe Store!";
            }
            else
            {
                var checkOrder = new Order
                {
                    CustomerName = CustomerName,
                    Address = Address,
                    Phone = Phone,
                    TotalAmount = TotalAmount,
                    PaymentMethod = "Momo",
                    OrderId = requestQuery["orderId"],
                    PaymentStatus = "Fail",
                    OrderStatus = "Procesing",
                    OrderDate = DateTime.Now
                };
                _dbContext.Orders.Add(checkOrder);
                await _dbContext.SaveChangesAsync();
                var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");

                if (cart == null || !cart.Any())
                {
                    ViewBag.Message = "Giỏ hàng của bạn đang trống.";
                    return RedirectToAction("Index", "Home");
                }

                SaveOrderDetails(cart, checkOrder.OrderId);
                ViewBag.Message = "Thanh toán thất bại. Vui lòng thử lại hoặc liên hệ hỗ trợ.";
                TempData["Error"] = "Thanh toán thất bại. Vui lòng thử lại hoặc liên hệ hỗ trợ.!";
            }
            return View("PaymentCallback", response);
        }
    }
}
