using Microsoft.EntityFrameworkCore;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;
using StoreManagement.Infrastructure.Data;
using StoreManagement.Infrastructure.Repositories;

namespace StoreManagement.Infrastructure.Repositories;

public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(StoreDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Payment>> GetByOrderAsync(int orderId)
    {
        return await _dbSet
            .Include(p => p.Order)
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    // Override GetPagedAsync để include Order navigation property
    public override async Task<(IEnumerable<Payment> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        System.Linq.Expressions.Expression<Func<Payment, bool>>? filter = null,
        Func<IQueryable<Payment>, IOrderedQueryable<Payment>>? orderBy = null)
    {
        IQueryable<Payment> query = _dbSet
            .Include(p => p.Order);

        // Apply filter if provided
        if (filter != null)
        {
            query = query.Where(filter);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply ordering if provided, otherwise order by PaymentDate descending
        if (orderBy != null)
        {
            query = orderBy(query);
        }
        else
        {
            query = query.OrderByDescending(p => p.PaymentDate);
        }

        // Apply pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
