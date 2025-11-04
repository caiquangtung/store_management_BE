using Microsoft.EntityFrameworkCore;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;
using StoreManagement.Infrastructure.Data;
using System.Linq.Expressions;
using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Threading.Tasks; 
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

    public async Task<IEnumerable<PurchaseItem>> GetLedgerMovementsAsync(int productId, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.PurchaseItems
            .Include(pi => pi.Purchase)
            .Where(pi => pi.ProductId == productId && pi.Purchase != null && pi.Purchase.Status == Domain.Enums.PurchaseStatus.Confirmed);

        if (startDate.HasValue)
        {
            query = query.Where(pi => pi.Purchase!.CreatedAt >= startDate.Value);
        }
        if (endDate.HasValue)
        {
            query = query.Where(pi => pi.Purchase!.CreatedAt < endDate.Value);
        }

        return await query.ToListAsync();
    }
    public async Task<IEnumerable<PurchaseSummaryRawData>> GetPurchaseSummaryAsync(DateTime startDate, DateTime endDate, string groupBy)
    {
        var query = _dbSet
            .Where(p => p.Status == Domain.Enums.PurchaseStatus.Confirmed &&
                        p.UpdatedAt >= startDate &&
                        p.UpdatedAt.Date <= endDate);

        if (groupBy.Equals("month", StringComparison.OrdinalIgnoreCase))
        {
            // 1. Lấy dữ liệu thô (Year, Month, Sum, Count) từ CSDL
            var data = await query
                .GroupBy(p => new { p.UpdatedAt.Year, p.UpdatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    TotalSpent = g.Sum(p => p.TotalAmount),
                    NumberOfPurchases = g.Count()
                })
                .OrderBy(r => r.Year).ThenBy(r => r.Month)
                .ToListAsync(); // <-- DỮ LIỆU ĐƯỢC LẤY VỀ BỘ NHỚ TẠI ĐÂY

            // 2. Định dạng (Format) dữ liệu bằng C# (trên Client-side)
            return data.Select(d => new PurchaseSummaryRawData
            {
                Period = $"{d.Year}-{d.Month:D2}", // <-- Định dạng C# an toàn
                TotalSpent = d.TotalSpent,
                NumberOfPurchases = d.NumberOfPurchases
            });
        }
        else
        {
            // 1. Lấy dữ liệu thô (Date, Sum, Count) từ CSDL
            var data = await query
                .GroupBy(p => p.UpdatedAt.Date) // Group by Date (EF Core có thể dịch)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalSpent = g.Sum(p => p.TotalAmount),
                    NumberOfPurchases = g.Count()
                })
                .OrderBy(r => r.Date)
                .ToListAsync(); // <-- DỮ LIỆU ĐƯỢC LẤY VỀ BỘ NHỚ TẠI ĐÂY

            // 2. Định dạng (Format) dữ liệu bằng C# (trên Client-side)
            return data.Select(d => new PurchaseSummaryRawData
            {
                Period = d.Date.ToString("yyyy-MM-dd"), // <-- Định dạng C# an toàn
                TotalSpent = d.TotalSpent,
                NumberOfPurchases = d.NumberOfPurchases
            });
        }
    }
}