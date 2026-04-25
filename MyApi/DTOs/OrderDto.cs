namespace MyApi.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public string Status { get; set; } = null!;
    }
}
