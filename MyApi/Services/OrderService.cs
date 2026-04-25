using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Entities;
using MyApi.Repositories;

namespace MyApi.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        public async Task<int> CreateOrderAsync(string productName, int quantity)
        {
            var order = new Order
            {
                ProductName = productName,
                Quantity = quantity,
                Status = "Pending"
            };

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            return order.Id;
        }

        public async Task<IEnumerable<Order>> GetAll()
        {
            return await _orderRepository.GetAllAsync();
        }

        public Task<Order?> GetByIdAsync(int id)
        {
            return _orderRepository.GetByIdAsync(id);
        }
    }
}
