using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace StoreManagement.Application.DTOs.Products;
using StoreManagement.Domain.Enums;
public class UpdateProductRequest
{
    [StringLength(100)]
    public string? ProductName { get; set; }
    public int? CategoryId { get; set; }
    public int? SupplierId { get; set; }
    [StringLength(50)]
    public string? Barcode { get; set; }
    [Range(0.01, double.MaxValue)]
    public decimal? Price { get; set; }
    [StringLength(20)]
    public string? Unit { get; set; }
    public IFormFile? Image { get; set; }
    public EntityStatus? Status { get; set; }  // New field for file update
}