namespace StoreManagement.Application.DTOs.Categories;
using StoreManagement.Domain.Enums;
public class CategoryResponse
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public EntityStatus Status { get; set; }
}