namespace StoreManagement.Application.DTOs.Reports;

public class PurchaseSummaryResponse
{
    public string Period { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    public int NumberOfPurchases { get; set; }
}