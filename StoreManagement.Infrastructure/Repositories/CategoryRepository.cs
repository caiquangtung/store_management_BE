using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;
using StoreManagement.Infrastructure.Data;
namespace StoreManagement.Infrastructure.Repositories;

public class CategoryRepository : BaseRepository<Category>, IRepository<Category>
{
    public CategoryRepository(StoreDbContext context) : base(context)
    {
    }
}