using StoreManagement.Application.DTOs.Reports;
using System; 
using System.Collections.Generic;
namespace StoreManagement.Application.Services;

public interface IReportService
{
    Task<IEnumerable<SalesSummaryResponse>> GetSalesOverviewAsync(DateTime startDate, DateTime endDate, string groupBy);
    Task<IEnumerable<DeadStockProductResponse>> GetDeadStockProductsAsync(DateTime startDate, DateTime endDate);
    Task<InventoryLedgerResponse> GetInventoryLedgerAsync(int productId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<PurchaseSummaryResponse>> GetPurchaseSummaryAsync(DateTime startDate, DateTime endDate, string groupBy);
}