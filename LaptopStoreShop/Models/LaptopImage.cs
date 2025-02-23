using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LaptopStoreShop.Models
{
    public class LaptopImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Laptop")]
        public int LaptopId { get; set; } 

        [Required, StringLength(255)]
        public string ImageUrl { get; set; }

        public byte Status { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.Now; 

        public Laptop Laptop { get; set; }
    }
}
