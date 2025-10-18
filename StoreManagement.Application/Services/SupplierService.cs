using AutoMapper;
using StoreManagement.Application.DTOs.Suppliers;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;

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

    public async Task<(IEnumerable<SupplierResponse> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize)
    {
        var (items, totalCount) = await _supplierRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            null,
            query => query.OrderBy(s => s.Name));

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

        await _supplierRepository.DeleteAsync(supplier);
        await _supplierRepository.SaveChangesAsync();
        return true;
    }
}