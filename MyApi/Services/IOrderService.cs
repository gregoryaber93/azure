using MyApi.Entities;

namespace MyApi.Services
{
    public interface IOrderService
    {
        Task<int> CreateOrderAsync(string productName, int quantity);
        Task<IEnumerable<Order>> GetAll();
        Task<Order?> GetByIdAsync(int id);
    }
}
