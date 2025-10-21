using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;

namespace StoreManagement.Domain.Interfaces;

public interface IInventoryRepository : IRepository<Inventory>
{
    Task<IEnumerable<Inventory>> GetAllWithProductAsync();
    Task<Inventory?> GetByIdWithProductAsync(int id);
    Task<Inventory?> GetByProductIdAsync(int productId); 
    Task<IEnumerable<Inventory>> GetLowStockAsync(int threshold);
}