using Microsoft.EntityFrameworkCore;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;
using StoreManagement.Infrastructure.Data;
using StoreManagement.Infrastructure.Repositories;
using System.Linq.Expressions;

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
            .Where(i => i.Quantity < threshold && i.Quantity >= 0)  // Only active stock
            .OrderBy(i => i.Quantity)  // Lowest first
            .ToListAsync();
    }

    public async Task<Inventory?> GetByProductIdAsync(int productId)
    {
        return await _dbSet
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.ProductId == productId);
    }

    // Override GetPagedAsync to include Product navigation properties
    public override async Task<(IEnumerable<Inventory> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<Inventory, bool>>? filter = null,
        Func<IQueryable<Inventory>, IOrderedQueryable<Inventory>>? orderBy = null)
    {
        IQueryable<Inventory> query = _dbSet
            .Include(i => i.Product)
                .ThenInclude(p => p.Category)
            .Include(i => i.Product)
                .ThenInclude(p => p.Supplier);

        // Apply filter if provided
        if (filter != null)
        {
            query = query.Where(filter);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply ordering if provided
        if (orderBy != null)
        {
            query = orderBy(query);
        }

        // Apply pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
