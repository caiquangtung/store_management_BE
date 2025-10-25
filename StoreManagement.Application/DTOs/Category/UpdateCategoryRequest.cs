namespace StoreManagement.Application.DTOs.Categories;
using StoreManagement.Domain.Enums;
public class UpdateCategoryRequest
{
    public string? CategoryName { get; set; }
    public EntityStatus? Status { get; set; }
}