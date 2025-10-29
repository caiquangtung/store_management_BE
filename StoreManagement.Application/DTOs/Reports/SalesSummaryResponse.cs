namespace StoreManagement.Application.DTOs.Reports;

public class SalesSummaryResponse
{
    public string Period { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int NumberOfOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
}