using Microsoft.EntityFrameworkCore;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;
using StoreManagement.Infrastructure.Data;
using StoreManagement.Infrastructure.Repositories;

namespace StoreManagement.Infrastructure.Repositories;

public class OrderRepository : BaseRepository<Order>, IOrderRepository
{
    public OrderRepository(StoreDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Order>> GetByCustomerAsync(int customerId)
    {
        return await _dbSet
            .Include(o => o.Customer)
            .Include(o => o.User)
            .Include(o => o.Promotion)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByUserAsync(int userId)
    {
        return await _dbSet
            .Include(o => o.Customer)
            .Include(o => o.User)
            .Include(o => o.Promotion)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
    {
        // Note: If you have OrderNumber field in Order entity, uncomment below
        // return await _dbSet.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        throw new NotImplementedException("OrderNumber field not implemented yet");
    }

    public async Task<bool> OrderNumberExistsAsync(string orderNumber)
    {
        // Note: If you have OrderNumber field in Order entity, uncomment below
        // return await _dbSet.AnyAsync(o => o.OrderNumber == orderNumber);
        throw new NotImplementedException("OrderNumber field not implemented yet");
    }

    public async Task<Order?> GetByIdWithDetailsAsync(int orderId)
    {
        return await _dbSet
            .Include(o => o.Customer)
            .Include(o => o.User)
            .Include(o => o.Promotion)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Category)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
    }

    // Override GetPagedAsync để include navigation properties
    public override async Task<(IEnumerable<Order> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        System.Linq.Expressions.Expression<Func<Order, bool>>? filter = null,
        Func<IQueryable<Order>, IOrderedQueryable<Order>>? orderBy = null)
    {
        IQueryable<Order> query = _dbSet
            .Include(o => o.Customer)
            .Include(o => o.User)
            .Include(o => o.Promotion)
            .Include(o => o.OrderItems);

        // Apply filter if provided
        if (filter != null)
        {
            query = query.Where(filter);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply ordering if provided, otherwise order by OrderDate descending
        if (orderBy != null)
        {
            query = orderBy(query);
        }
        else
        {
            query = query.OrderByDescending(o => o.OrderDate);
        }

        // Apply pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
