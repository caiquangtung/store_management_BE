using AutoMapper;
using StoreManagement.Application.DTOs.Suppliers;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;
using System.Linq.Expressions;
using StoreManagement.Domain.Enums;
namespace StoreManagement.Application.Services;

public class SupplierService : ISupplierService
{
    private readonly IRepository<Supplier> _supplierRepository;
    private readonly IMapper _mapper;

    public SupplierService(IRepository<Supplier> supplierRepository, IMapper mapper)
    {
        _supplierRepository = supplierRepository;
        _mapper = mapper;
    }

    public async Task<SupplierResponse?> GetByIdAsync(int id)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        return supplier != null ? _mapper.Map<SupplierResponse>(supplier) : null;
    }

    public async Task<IEnumerable<SupplierResponse>> GetAllAsync()
    {
        var suppliers = await _supplierRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<SupplierResponse>>(suppliers);
    }

    public async Task<(IEnumerable<SupplierResponse> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize, EntityStatus? status = null, string? searchTerm = null, string? sortBy = null, bool sortDesc = false)
    {
        // Build filter expression
        Expression<Func<Supplier, bool>> filter = s =>
            (!status.HasValue || s.Status == status.Value) &&
            (string.IsNullOrEmpty(searchTerm) ||
                s.Name.Contains(searchTerm) ||
                (s.Email != null && s.Email.Contains(searchTerm)) ||
                (s.Phone != null && s.Phone.Contains(searchTerm)));

        Expression<Func<Supplier, object>> primarySort = (sortBy ?? string.Empty).ToLower() switch
        {
            "id" => s => s.SupplierId,
            "name" => s => s.Name,
            "email" => s => s.Email ?? string.Empty,
            _ => s => s.SupplierId
        };

        Func<IQueryable<Supplier>, IOrderedQueryable<Supplier>> orderBy = q =>
        {
            var ordered = sortDesc ? q.OrderByDescending(primarySort) : q.OrderBy(primarySort);
            return sortDesc ? ordered.ThenByDescending(s => s.SupplierId) : ordered.ThenBy(s => s.SupplierId);
        };

        var (items, totalCount) = await _supplierRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter,
            orderBy);

        var mappedItems = _mapper.Map<IEnumerable<SupplierResponse>>(items);
        return (mappedItems, totalCount);
    }

    public async Task<SupplierResponse?> CreateAsync(CreateSupplierRequest request)
    {
        var supplier = _mapper.Map<Supplier>(request);
        var createdSupplier = await _supplierRepository.AddAsync(supplier);
        await _supplierRepository.SaveChangesAsync();
        return _mapper.Map<SupplierResponse>(createdSupplier);
    }

    public async Task<SupplierResponse?> UpdateAsync(int id, UpdateSupplierRequest request)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        if (supplier == null)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            supplier.Name = request.Name;
        }
        if (!string.IsNullOrEmpty(request.Phone))
        {
            supplier.Phone = request.Phone;
        }
        if (!string.IsNullOrEmpty(request.Email))
        {
            supplier.Email = request.Email;
        }
        if (!string.IsNullOrEmpty(request.Address))
        {
            supplier.Address = request.Address;
        }
        if (request.Status.HasValue)
        {
            supplier.Status = request.Status.Value;
        }

        var updatedSupplier = await _supplierRepository.UpdateAsync(supplier);
        await _supplierRepository.SaveChangesAsync();
        return _mapper.Map<SupplierResponse>(updatedSupplier);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        if (supplier == null)
        {
            return false;
        }
        supplier.Status = EntityStatus.Deleted;
        await _supplierRepository.UpdateAsync(supplier);
        await _supplierRepository.SaveChangesAsync();
        return true;
    }
}