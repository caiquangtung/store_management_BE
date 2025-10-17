namespace StoreManagement.Application.DTOs.Products;

public class ABCProductResponse
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal Value { get; set; }  // Total revenue (SUM(price * quantity))
    public int Frequency { get; set; }  // Number of orders containing the product
    public decimal Score { get; set; }  // Value * Frequency
    public string ABCClassification { get; set; } = string.Empty;  // "A", "B", or "C"
}