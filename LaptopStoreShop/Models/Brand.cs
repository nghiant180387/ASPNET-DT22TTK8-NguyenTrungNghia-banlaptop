using System.ComponentModel.DataAnnotations;

namespace LaptopStoreShop.Models
{
    public class Brand
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string BrandName { get; set; }

        public byte Status { get; set; } = 1;
        public ICollection<Laptop> Laptops { get; set; } = new List<Laptop>();
    }
}
