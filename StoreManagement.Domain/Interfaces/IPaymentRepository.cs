using StoreManagement.Domain.Entities;

namespace StoreManagement.Domain.Interfaces;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<IEnumerable<Payment>> GetByOrderAsync(int orderId);
}
