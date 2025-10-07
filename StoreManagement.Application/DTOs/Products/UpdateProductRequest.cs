namespace StoreManagement.Application.DTOs.Products;

public class UpdateProductRequest
{
    public int? CategoryId { get; set; }
    public int? SupplierId { get; set; }
    public string? ProductName { get; set; }
    public string? Barcode { get; set; }
    public decimal? Price { get; set; }
    public string? Unit { get; set; }
}