using LaptopStoreShop.Data;
using LaptopStoreShop.Models;
using LaptopStoreShop.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
public class LoginController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly EmailService _emailService;
    public LoginController(ApplicationDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
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

             return RedirectToAction("Index", "Home");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View("Index");
        }
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
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
    public IActionResult ForgotPassword()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            return Json(new { success = false, message = "Email không tồn tại!" });
        }

        user.ResetPasswordToken = Guid.NewGuid().ToString();
        user.ResetTokenExpires = DateTime.Now.AddHours(1);
        await _context.SaveChangesAsync();

        string resetLink = Url.Action("ResetPassword", "Login", new { token = user.ResetPasswordToken }, Request.Scheme);

        await _emailService.SendEmailAsync(user.Email, "Đặt lại mật khẩu", $"Click vào link để đặt lại mật khẩu: {resetLink}");

        TempData["Message"] = "Hướng dẫn đặt lại mật khẩu đã được gửi đến email của bạn.";
        return RedirectToAction("Index", "Home");
    }
    [HttpGet]
    public async Task<IActionResult> ResetPassword(string token)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetPasswordToken == token && u.ResetTokenExpires > DateTime.Now);

        if (user == null)
        {
            return View("Error");
        }

        return View(new ResetPassword { Token = token });
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPassword model)
    {
        if (ModelState.IsValid)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.ResetPasswordToken == model.Token);

            if (user != null)
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
                user.ResetPasswordToken = null;
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Login");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Token không hợp lệ hoặc đã hết hạn.");
            }
        }

        return View(model);
    }
}
