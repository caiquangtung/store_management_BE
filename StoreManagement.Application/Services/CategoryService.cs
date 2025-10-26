using AutoMapper;
using StoreManagement.Application.DTOs.Categories;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;
using System.Linq.Expressions;
using StoreManagement.Domain.Enums;
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

    public async Task<(IEnumerable<CategoryResponse> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize, EntityStatus? status = null, string? searchTerm = null, string? sortBy = null, bool sortDesc = false)
    {
        // Build filter expression
        Expression<Func<Category, bool>> filter = c =>
            (!status.HasValue || c.Status == status.Value) &&
            (string.IsNullOrEmpty(searchTerm) || c.CategoryName.Contains(searchTerm));

        Expression<Func<Category, object>> primarySort = (sortBy ?? string.Empty).ToLower() switch
        {
            "id" => c => c.CategoryId,
            "name" or "categoryname" => c => c.CategoryName,
            _ => c => c.CategoryId
        };

        Func<IQueryable<Category>, IOrderedQueryable<Category>> orderBy = q =>
        {
            var ordered = sortDesc ? q.OrderByDescending(primarySort) : q.OrderBy(primarySort);
            return sortDesc ? ordered.ThenByDescending(c => c.CategoryId) : ordered.ThenBy(c => c.CategoryId);
        };

        var (items, totalCount) = await _categoryRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter,
            orderBy);

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

        if (request.Status.HasValue)
        {
            category.Status = request.Status.Value;
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

        category.Status = EntityStatus.Deleted;
        await _categoryRepository.UpdateAsync(category);
        await _categoryRepository.SaveChangesAsync();
        return true;
    }
}