using AutoMapper;
using StoreManagement.Application.DTOs.Reports;
using StoreManagement.Domain.Interfaces;

namespace StoreManagement.Application.Services;

public class ReportService : IReportService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public ReportService(IOrderRepository orderRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<SalesSummaryResponse>> GetSalesOverviewAsync(DateTime startDate, DateTime endDate, string groupBy)
    {
        var rawData = await _orderRepository.GetSalesOverviewAsync(startDate, endDate, groupBy);
        
        return rawData.Select(d => new SalesSummaryResponse
        {
            Period = d.Period,
            TotalRevenue = d.TotalRevenue,
            NumberOfOrders = d.NumberOfOrders,
            AverageOrderValue = (d.NumberOfOrders > 0) ? d.TotalRevenue / d.NumberOfOrders : 0
        });
    }

    public async Task<IEnumerable<DeadStockProductResponse>> GetDeadStockProductsAsync(DateTime startDate, DateTime endDate)
    {
        var deadStockProducts = await _productRepository.GetDeadStockProductsAsync(startDate, endDate);
        
        return deadStockProducts.Select(p => new DeadStockProductResponse
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            Barcode = p.Barcode,
            Price = p.Price,
            QuantityInStock = p.Inventory.FirstOrDefault()?.Quantity ?? 0
        });
    }
}