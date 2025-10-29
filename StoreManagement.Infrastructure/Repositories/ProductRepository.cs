using Microsoft.EntityFrameworkCore;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;
using StoreManagement.Infrastructure.Data;
using StoreManagement.Infrastructure.Repositories;

namespace StoreManagement.Infrastructure.Repositories;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(StoreDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Where(p => p.CategoryId == categoryId)
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetBySupplierAsync(int supplierId)
    {
        return await _dbSet
            .Where(p => p.SupplierId == supplierId)
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .ToListAsync();
    }

    public async Task<Product?> GetBySKUAsync(string sku)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Barcode == sku);
    }

    public async Task<bool> SKUExistsAsync(string sku)
    {
        return await _dbSet.AnyAsync(p => p.Barcode == sku);
    }

    public async Task<IEnumerable<ABCData>> GetABCAnalysisDataAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = from oi in _context.OrderItems
                    join o in _context.Orders on oi.OrderId equals o.OrderId
                    join p in _context.Products on oi.ProductId equals p.ProductId
                    where (fromDate == null || o.OrderDate >= fromDate.Value) && (toDate == null || o.OrderDate <= toDate.Value)
                    group new { oi, p } by p.ProductId into g
                    select new ABCData
                    {
                        ProductId = g.Key,
                        ProductName = g.First().p.ProductName,
                        Barcode = g.First().p.Barcode,
                        Value = g.Sum(x => x.oi.Price * x.oi.Quantity),  // SUM(price * quantity)
                        Frequency = g.Select(x => x.oi.OrderId).Distinct().Count(),  // COUNT(DISTINCT order_id)
                        Score = (decimal)(g.Sum(x => x.oi.Price * x.oi.Quantity) * g.Select(x => x.oi.OrderId).Distinct().Count())  // value * frequency (cast to decimal, null-safe by SUM)
                    };

        return await query.ToListAsync();
    }
    public async Task<IEnumerable<Product>> GetDeadStockProductsAsync(DateTime startDate, DateTime endDate)
    {
        // Lấy danh sách ID các sản phẩm đã được bán trong khoảng thời gian
        var soldProductIds = await _context.OrderItems
            .Include(oi => oi.Order)
            .Where(oi => oi.Order != null && oi.Order.OrderDate >= startDate && oi.Order.OrderDate <= endDate)
            .Select(oi => oi.ProductId)
            .Distinct()
            .ToListAsync();

        // Lấy các sản phẩm không nằm trong danh sách đã bán
        return await _dbSet
            .Include(p => p.Inventory) // Lấy thông tin tồn kho
            .Where(p => !soldProductIds.Contains(p.ProductId))
            .ToListAsync();
    }
}