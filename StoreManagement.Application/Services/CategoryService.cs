using AutoMapper;
using StoreManagement.Application.DTOs.Categories;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;
using System.Linq.Expressions;

namespace StoreManagement.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IRepository<Category> _categoryRepository;
    private readonly IMapper _mapper;

    public CategoryService(IRepository<Category> categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<CategoryResponse?> GetByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category != null ? _mapper.Map<CategoryResponse>(category) : null;
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CategoryResponse>>(categories);
    }

    public async Task<(IEnumerable<CategoryResponse> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize, string? searchTerm = null)
    {
        // Build filter expression
        Expression<Func<Category, bool>>? filter = null;
        if (!string.IsNullOrEmpty(searchTerm))
        {
            filter = c => c.CategoryName.Contains(searchTerm);
        }

        var (items, totalCount) = await _categoryRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter,
            query => query.OrderBy(c => c.CategoryName));

        var mappedItems = _mapper.Map<IEnumerable<CategoryResponse>>(items);
        return (mappedItems, totalCount);
    }

    public async Task<CategoryResponse?> CreateAsync(CreateCategoryRequest request)
    {
        var category = _mapper.Map<Category>(request);
        var createdCategory = await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveChangesAsync();
        return _mapper.Map<CategoryResponse>(createdCategory);
    }

    public async Task<CategoryResponse?> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(request.CategoryName))
        {
            category.CategoryName = request.CategoryName;
        }

        var updatedCategory = await _categoryRepository.UpdateAsync(category);
        await _categoryRepository.SaveChangesAsync();
        return _mapper.Map<CategoryResponse>(updatedCategory);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return false;
        }

        await _categoryRepository.DeleteAsync(category);
        await _categoryRepository.SaveChangesAsync();
        return true;
    }
}