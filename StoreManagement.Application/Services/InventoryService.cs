using AutoMapper;
using StoreManagement.Application.DTOs.Inventory;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;
using System.Linq.Expressions;

namespace StoreManagement.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IMapper _mapper;

    public InventoryService(IInventoryRepository inventoryRepository, IMapper mapper)
    {
        _inventoryRepository = inventoryRepository;
        _mapper = mapper;
    }

    public async Task<InventoryResponse?> GetByIdAsync(int id)
    {
        var inventory = await _inventoryRepository.GetByIdWithProductAsync(id);
        return inventory != null ? _mapper.Map<InventoryResponse>(inventory) : null;
    }

    public async Task<IEnumerable<InventoryResponse>> GetAllAsync()
    {
        var inventories = await _inventoryRepository.GetAllWithProductAsync();
        return _mapper.Map<IEnumerable<InventoryResponse>>(inventories);
    }

    public async Task<(IEnumerable<InventoryResponse> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize, int? productId = null, string? sortBy = null, bool sortDesc = false)
    {
        // Build filter expression
        Expression<Func<Inventory, bool>>? filter = null;
        if (productId.HasValue)
        {
            filter = i => i.ProductId == productId.Value;
        }

        // Note: For inventory we might want to use GetAllWithProductAsync for includes
        // But for pagination we'll use base GetPagedAsync and the mapper should handle navigation properties
        Expression<Func<Inventory, object>> primarySort = (sortBy ?? string.Empty).ToLower() switch
        {
            "id" => i => i.InventoryId,
            "productid" => i => i.ProductId,
            "quantity" => i => i.Quantity,
            _ => i => i.InventoryId
        };

        Func<IQueryable<Inventory>, IOrderedQueryable<Inventory>> orderBy = q =>
        {
            var ordered = sortDesc ? q.OrderByDescending(primarySort) : q.OrderBy(primarySort);
            return sortDesc ? ordered.ThenByDescending(i => i.InventoryId) : ordered.ThenBy(i => i.InventoryId);
        };

        var (items, totalCount) = await _inventoryRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter,
            orderBy);

        var mappedItems = _mapper.Map<IEnumerable<InventoryResponse>>(items);
        return (mappedItems, totalCount);
    }

    public async Task<InventoryResponse?> CreateAsync(CreateInventoryRequest request)
    {
        // Validate: Product must exist (assume checked in controller/validator)
        var inventory = _mapper.Map<Inventory>(request);
        var createdInventory = await _inventoryRepository.AddAsync(inventory);
        await _inventoryRepository.SaveChangesAsync();
        return _mapper.Map<InventoryResponse>(createdInventory);
    }

    public async Task<InventoryResponse?> UpdateAsync(int id, UpdateInventoryRequest request)
    {
        var inventory = await _inventoryRepository.GetByIdWithProductAsync(id);
        if (inventory == null)
        {
            return null;
        }

        // Update quantity (must be >= 0)
        if (request.Quantity < 0)
        {
            throw new InvalidOperationException("Quantity cannot be negative");
        }

        inventory.Quantity = request.Quantity;
        var updatedInventory = await _inventoryRepository.UpdateAsync(inventory);
        await _inventoryRepository.SaveChangesAsync();

        return _mapper.Map<InventoryResponse>(updatedInventory);
    }

    public async Task<bool> SetQuantityToZeroAsync(int id)
    {
        var inventory = await _inventoryRepository.GetByIdWithProductAsync(id);
        if (inventory == null)
        {
            return false;
        }

        inventory.Quantity = 0;  // Soft "delete" by setting to zero
        await _inventoryRepository.UpdateAsync(inventory);
        await _inventoryRepository.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<LowStockResponse>> GetLowStockAsync(int threshold = 10)
    {
        if (threshold < 1)
        {
            throw new InvalidOperationException("Threshold must be positive");
        }

        var lowStockItems = await _inventoryRepository.GetLowStockAsync(threshold);

        // Manual mapping to set Threshold, ReorderQuantity, IsLowStock
        var responses = lowStockItems.Select(item => new LowStockResponse
        {
            InventoryId = item.InventoryId,
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            Threshold = threshold,
            ReorderQuantity = threshold - item.Quantity,
            IsLowStock = item.Quantity < threshold,
            UpdatedAt = item.UpdatedAt,
            Product = _mapper.Map<ProductInfo>(item.Product)
        }).ToList();

        return responses;
    }
}