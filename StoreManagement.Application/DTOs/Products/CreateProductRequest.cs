using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace StoreManagement.Application.DTOs.Products;

public class CreateProductRequest
{
    [Required]
    public int? CategoryId { get; set; }
    [Required]
    public int? SupplierId { get; set; }
    [Required]
    [StringLength(100)]
    public string ProductName { get; set; } = string.Empty;
    [StringLength(50)]
    public string? Barcode { get; set; }
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
    [StringLength(20)]
    public string Unit { get; set; } = "pcs";
    public IFormFile? Image { get; set; }  // New field for file upload
}