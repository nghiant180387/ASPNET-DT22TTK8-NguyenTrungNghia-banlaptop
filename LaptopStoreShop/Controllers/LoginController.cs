using LaptopStoreShop.Data;
using LaptopStoreShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
public class LoginController : Controller
{
    private readonly ApplicationDbContext _context;
    public LoginController(ApplicationDbContext context)
    {
        _context = context;
    }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users
        .Where(a => a.Email == email)
        .Select(a => new
        {
            a.Id,
            a.Email,
            a.PasswordHash,
            a.FullName,
            a.Role,
            a.PhoneNumber,
            a.Address,
            a.Status
        })
        .SingleOrDefaultAsync();
            if (user != null)
            {
                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt. Incorrect password.");
                    return View();
                }

                HttpContext.Session.SetString("Name", user.FullName);
                HttpContext.Session.SetString("Role", user.Role);

                if (user.Role == "Admin")
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                else if (user.Role == "User")
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View("Index");
            }

            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Register(string password, string fullname, string email)
        {
            var existingEmail = await _context.Users
                                               .FirstOrDefaultAsync(u => u.Email == email);

            if (existingEmail != null)
            {
                ModelState.AddModelError("Email", "Email đã được đăng ký.");
                return View();
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var newAccount = new User
            {
                FullName = fullname,
                PasswordHash = passwordHash,
                Email = email,
                CreatedAt = DateTime.Now,
                Status = 1,
                Role = "User"
            };

            _context.Users.Add(newAccount);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Login");
        }
    }

