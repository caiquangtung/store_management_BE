using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;
using StoreManagement.Infrastructure.Data;

namespace StoreManagement.Infrastructure.Repositories;

public class SupplierRepository : BaseRepository<Supplier>, IRepository<Supplier>
{
    public SupplierRepository(StoreDbContext context) : base(context)
    {
    }
}