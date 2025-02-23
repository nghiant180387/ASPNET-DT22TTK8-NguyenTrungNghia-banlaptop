using LaptopStoreShop.Data;
using LaptopStoreShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaptopStoreShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/account")]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Select( x => new UserViewModel
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber ?? "Trống",
                    Address = x.Address ?? "Trống",
                    Role = x.Role,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
            return View(users);
        }
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create(User user)
        {
            _context.Add(user);
            await _context.SaveChangesAsync();
            if (user.FullName == null || user.Email == null || user.PasswordHash == null)
            {
                TempData["error"] = "Vui lòng nhập đầy đủ thông tin";
                return View(user);
            }
            TempData["success"] = "Thêm tài khoản thành công";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet("edit/{id}")]
        public IActionResult Edit(int id)
        {
            var user = _context.Users.Find(id);
            return View(user);
        }
        [HttpPost("edit/{id}")]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            try
            {
                var existingAccount = await _context.Users.FindAsync(id);
                if (existingAccount == null)
                {
                    return NotFound();
                }

                existingAccount.FullName = user.FullName;
                existingAccount.PhoneNumber = user.PhoneNumber;
                existingAccount.Status = user.Status;
                existingAccount.UpdatedAt = DateTime.Now;

                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    existingAccount.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(e => e.Id == user.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            TempData["success"] = "Cập nhật tài khoản thành công";
            return RedirectToAction("Index");
        }
        [Route("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            TempData["success"] = "Xóa tài khoản thành công";
            return RedirectToAction(nameof(Index));
        }
    }
}
