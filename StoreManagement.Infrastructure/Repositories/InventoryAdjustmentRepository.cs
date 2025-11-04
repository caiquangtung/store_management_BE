using Microsoft.EntityFrameworkCore;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;
using StoreManagement.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StoreManagement.Infrastructure.Repositories;

public class InventoryAdjustmentRepository : BaseRepository<InventoryAdjustment>, IInventoryAdjustmentRepository
{
    public InventoryAdjustmentRepository(StoreDbContext context) : base(context)
    {
    }
    
    // Có thể override GetPagedAsync để Include Product và User nếu cần
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