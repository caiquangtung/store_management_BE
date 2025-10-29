// StoreManagement.Application/DTOs/Reports/DeadStockProductResponse.cs
namespace StoreManagement.Application.DTOs.Reports;

public class DeadStockProductResponse
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal Price { get; set; }
    public int QuantityInStock { get; set; }
}