namespace MyApi.Entities
{
    public class OrderEvent
    {
        public string id { get; set; } = Guid.NewGuid().ToString();
        public string orderId { get; set; }
        public string type { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
    }
}
