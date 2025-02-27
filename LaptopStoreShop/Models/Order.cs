namespace LaptopStoreShop.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderId { get; set; }
        public string CustomerName { get; set; } 
        public string Address { get; set; } 
        public string Phone { get; set; }
        public string PaymentMethod { get; set; } 
        public string PaymentStatus { get; set; }
        public string OrderStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; } 

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
