using StoreManagement.Domain.Entities;

namespace StoreManagement.Domain.Interfaces;

public interface IInventoryAdjustmentRepository : IRepository<InventoryAdjustment>
{
    Task<IEnumerable<InventoryAdjustment>> GetLedgerMovementsAsync(int productId, DateTime? startDate, DateTime? endDate);
}