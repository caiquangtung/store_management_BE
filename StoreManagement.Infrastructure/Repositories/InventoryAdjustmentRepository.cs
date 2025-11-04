using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;
using StoreManagement.Infrastructure.Data;

namespace StoreManagement.Infrastructure.Repositories;

public class InventoryAdjustmentRepository : BaseRepository<InventoryAdjustment>, IInventoryAdjustmentRepository
{
    public InventoryAdjustmentRepository(StoreDbContext context) : base(context)
    {
    }
    
    // Có thể override GetPagedAsync để Include Product và User nếu cần
}