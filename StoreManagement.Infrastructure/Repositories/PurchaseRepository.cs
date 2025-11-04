using Microsoft.EntityFrameworkCore;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;
using StoreManagement.Infrastructure.Data;
using System.Linq.Expressions;

namespace StoreManagement.Infrastructure.Repositories;

public class PurchaseRepository : BaseRepository<Purchase>, IPurchaseRepository
{
    public PurchaseRepository(StoreDbContext context) : base(context)
    {
    }

    public async Task<Purchase?> GetByIdWithDetailsAsync(int purchaseId)
    {
        return await _dbSet
            .Include(p => p.Supplier)
            .Include(p => p.User)
            .Include(p => p.PurchaseItems)
                .ThenInclude(pi => pi.Product)
            .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId);
    }

    public override async Task<(IEnumerable<Purchase> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<Purchase, bool>>? filter = null,
        Func<IQueryable<Purchase>, IOrderedQueryable<Purchase>>? orderBy = null)
    {
        // Bắt đầu query với Includes
        IQueryable<Purchase> query = _dbSet
            .Include(p => p.Supplier)
            .Include(p => p.User);

        // Áp dụng filter (nếu có)
        if (filter != null)
        {
            query = query.Where(filter);
        }

        // Đếm tổng số lượng (sau khi filter)
        var totalCount = await query.CountAsync();

        // Áp dụng sắp xếp (nếu có)
        if (orderBy != null)
        {
            query = orderBy(query);
        }
        else
        {
            // Sắp xếp mặc định
            query = query.OrderByDescending(p => p.CreatedAt);
        }

        // Áp dụng phân trang
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}