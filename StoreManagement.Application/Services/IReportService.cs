// StoreManagement.Application/Services/IReportService.cs
using StoreManagement.Application.DTOs.Reports;

namespace StoreManagement.Application.Services;

public interface IReportService
{
    Task<IEnumerable<SalesSummaryResponse>> GetSalesOverviewAsync(DateTime startDate, DateTime endDate, string groupBy);
    Task<IEnumerable<DeadStockProductResponse>> GetDeadStockProductsAsync(DateTime startDate, DateTime endDate);
}