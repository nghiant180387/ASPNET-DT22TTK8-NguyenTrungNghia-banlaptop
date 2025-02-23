using System.ComponentModel.DataAnnotations;

namespace LaptopStoreShop.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, StringLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [StringLength(15)]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string Role { get; set; } = "User";

        public byte Status { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } 
        public string? ResetPasswordToken { get; set; }

        public DateTime? ResetTokenExpires { get; set; }
    }
}
