using Microsoft.EntityFrameworkCore;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;
using StoreManagement.Infrastructure.Data;
using StoreManagement.Infrastructure.Repositories;
using System.Linq.Expressions;

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
    public async Task<IEnumerable<SalesSummaryRawData>> GetSalesOverviewAsync(DateTime startDate, DateTime endDate, string groupBy)
    {
        var query = _dbSet
            .Where(o => o.Status == Domain.Enums.OrderStatus.Paid && o.OrderDate >= startDate && o.OrderDate.Date <= endDate);

        if (groupBy.Equals("month", StringComparison.OrdinalIgnoreCase))
        {
            var data = await query
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    TotalRevenue = g.Sum(o => (o.TotalAmount ?? 0) - o.DiscountAmount),
                    NumberOfOrders = g.Count()
                })
                .OrderBy(r => r.Year).ThenBy(r => r.Month)
                .ToListAsync();

            // Định dạng chuỗi Period ở phía ứng dụng
            return data.Select(d => new SalesSummaryRawData
            {
                Period = $"{d.Year}-{d.Month:D2}",
                TotalRevenue = d.TotalRevenue,
                NumberOfOrders = d.NumberOfOrders
            });
        }
        else // Mặc định là 'day'
        {
            var data = await query
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalRevenue = g.Sum(o => (o.TotalAmount ?? 0) - o.DiscountAmount),
                    NumberOfOrders = g.Count()
                })
                .OrderBy(r => r.Date)
                .ToListAsync();

            // Định dạng chuỗi Period ở phía ứng dụng
            return data.Select(d => new SalesSummaryRawData
            {
                Period = d.Date.ToString("yyyy-MM-dd"),
                TotalRevenue = d.TotalRevenue,
                NumberOfOrders = d.NumberOfOrders
            });
        }
    }
    
    public async Task<IEnumerable<OrderItem>> GetLedgerMovementsAsync(int productId, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.OrderItems
            .Include(oi => oi.Order)
            .Where(oi => oi.ProductId == productId && oi.Order != null && oi.Order.Status == Domain.Enums.OrderStatus.Paid);

        if (startDate.HasValue)
        {
            query = query.Where(oi => oi.Order!.OrderDate >= startDate.Value);
        }
        if (endDate.HasValue)
        {
            query = query.Where(oi => oi.Order!.OrderDate < endDate.Value);
        }

        return await query.ToListAsync();
    }
}
