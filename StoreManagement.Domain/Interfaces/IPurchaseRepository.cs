using StoreManagement.Domain.Entities;

namespace StoreManagement.Domain.Interfaces;

public interface IPurchaseRepository : IRepository<Purchase>
{
    Task<Purchase?> GetByIdWithDetailsAsync(int purchaseId);
}