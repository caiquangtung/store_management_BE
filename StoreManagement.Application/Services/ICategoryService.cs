using StoreManagement.Application.DTOs.Categories;

namespace StoreManagement.Application.Services;

public interface ICategoryService
{
    Task<CategoryResponse?> GetByIdAsync(int id);
    Task<IEnumerable<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse?> CreateAsync(CreateCategoryRequest request);
    Task<CategoryResponse?> UpdateAsync(int id, UpdateCategoryRequest request);
    Task<bool> DeleteAsync(int id);
}