using StoreManagement.Application.DTOs.Categories;
using StoreManagement.Domain.Enums;
namespace StoreManagement.Application.Services;

public interface ICategoryService
{
    Task<CategoryResponse?> GetByIdAsync(int id);
    Task<IEnumerable<CategoryResponse>> GetAllAsync();
    Task<(IEnumerable<CategoryResponse> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize, EntityStatus? status = null, string? searchTerm = null, string? sortBy = null, bool sortDesc = false);
    Task<CategoryResponse?> CreateAsync(CreateCategoryRequest request);
    Task<CategoryResponse?> UpdateAsync(int id, UpdateCategoryRequest request);
    Task<bool> DeleteAsync(int id);
}