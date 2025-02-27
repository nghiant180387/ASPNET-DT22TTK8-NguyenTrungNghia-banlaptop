using System.ComponentModel.DataAnnotations;

namespace LaptopStoreShop.Models
{
    public class ResetPassword
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Mật khẩu phải ít nhất {2} ký tự.", MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }
    }
}
