using Microsoft.EntityFrameworkCore;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;
using StoreManagement.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StoreManagement.Infrastructure.Repositories;

public class InventoryAdjustmentRepository : BaseRepository<InventoryAdjustment>, IInventoryAdjustmentRepository
{
    public InventoryAdjustmentRepository(StoreDbContext context) : base(context)
    {
    }
    
    // Override GetPagedAsync để Include Product và User
    public override async Task<(IEnumerable<InventoryAdjustment> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<InventoryAdjustment, bool>>? filter = null,
        Func<IQueryable<InventoryAdjustment>, IOrderedQueryable<InventoryAdjustment>>? orderBy = null)
    {
        IQueryable<InventoryAdjustment> query = _dbSet
            .Include(a => a.Product)  // Include Product để lấy ProductName
            .Include(a => a.User);    // Include User để lấy FullName

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

    // Override GetByIdAsync để Include Product và User
    public override async Task<InventoryAdjustment?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(a => a.Product)
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.AdjustmentId == id);
    }

    public async Task<IEnumerable<InventoryAdjustment>> GetLedgerMovementsAsync(int productId, DateTime? startDate, DateTime? endDate)
    {
        var query = _dbSet
            .Include(a => a.User) // Include User để lấy tên
            .Where(a => a.ProductId == productId);

        if (startDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt >= startDate.Value);
        }
        if (endDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt < endDate.Value);
        }

        return await query.ToListAsync();
    }
}