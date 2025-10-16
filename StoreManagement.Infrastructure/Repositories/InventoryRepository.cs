using Microsoft.EntityFrameworkCore;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;
using StoreManagement.Infrastructure.Data;
using StoreManagement.Infrastructure.Repositories;

namespace StoreManagement.Infrastructure.Repositories;

public class InventoryRepository : BaseRepository<Inventory>, IInventoryRepository
{
    public InventoryRepository(StoreDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Inventory>> GetAllWithProductAsync()
    {
        return await _dbSet
            .Include(i => i.Product)
                .ThenInclude(p => p.Category)
            .Include(i => i.Product)
                .ThenInclude(p => p.Supplier)
            .OrderBy(i => i.Product.ProductName)
            .ToListAsync();
    }

    public async Task<Inventory?> GetByIdWithProductAsync(int id)
    {
        return await _dbSet
            .Include(i => i.Product)
                .ThenInclude(p => p.Category)
            .Include(i => i.Product)
                .ThenInclude(p => p.Supplier)
            .FirstOrDefaultAsync(i => i.InventoryId == id);
    }

    public async Task<IEnumerable<Inventory>> GetLowStockAsync(int threshold)
    {
        return await _dbSet
            .Include(i => i.Product)
                .ThenInclude(p => p.Category)
            .Include(i => i.Product)
                .ThenInclude(p => p.Supplier)
            .Where(i => i.Quantity < threshold && i.Quantity > 0)  // Only active stock
            .OrderBy(i => i.Quantity)  // Lowest first
            .ToListAsync();
    }
}