using LaptopStoreShop.Models;
using Microsoft.EntityFrameworkCore;

namespace LaptopStoreShop.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Laptop> Laptops { get; set; }
        public DbSet<User> Users { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Laptop>()
                .HasOne(l => l.Brand)
                .WithMany(b => b.Laptops)
                .HasForeignKey(l => l.BrandId);

            modelBuilder.Entity<Laptop>()
                .HasOne(l => l.Category)
                .WithMany(c => c.Laptops)
                .HasForeignKey(l => l.CategoryId);
        }
    }
}
