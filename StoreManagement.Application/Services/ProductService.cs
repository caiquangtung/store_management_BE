using AutoMapper;
using StoreManagement.Application.DTOs.Products;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;

namespace StoreManagement.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public ProductService(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<ProductResponse?> GetByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product != null ? _mapper.Map<ProductResponse>(product) : null;
    }

    public async Task<IEnumerable<ProductResponse>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ProductResponse>>(products);
    }

    public async Task<ProductResponse?> CreateAsync(CreateProductRequest request)
    {
        // Check if SKU already exists
        if (!string.IsNullOrEmpty(request.Barcode) && await _productRepository.SKUExistsAsync(request.Barcode))
        {
            throw new InvalidOperationException("SKU already exists");
        }

        // Map DTO to entity
        var product = _mapper.Map<Product>(request);
        product.CreatedAt = DateTime.UtcNow;

        // Add to repository
        var createdProduct = await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        return _mapper.Map<ProductResponse>(createdProduct);
    }

    public async Task<ProductResponse?> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return null;
        }

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.ProductName))
        {
            product.ProductName = request.ProductName;
        }
        if (request.CategoryId.HasValue)
        {
            product.CategoryId = request.CategoryId;
        }
        if (request.SupplierId.HasValue)
        {
            product.SupplierId = request.SupplierId;
        }
        if (!string.IsNullOrEmpty(request.Barcode))
        {
            if (await _productRepository.SKUExistsAsync(request.Barcode) && request.Barcode != product.Barcode)
            {
                throw new InvalidOperationException("SKU already exists");
            }
            product.Barcode = request.Barcode;
        }
        if (request.Price.HasValue)
        {
            product.Price = request.Price.Value;
        }
        if (!string.IsNullOrEmpty(request.Unit))
        {
            product.Unit = request.Unit;
        }

        var updatedProduct = await _productRepository.UpdateAsync(product);
        await _productRepository.SaveChangesAsync();

        return _mapper.Map<ProductResponse>(updatedProduct);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return false;
        }

        await _productRepository.DeleteAsync(product);
        await _productRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SKUExistsAsync(string sku)
    {
        return await _productRepository.SKUExistsAsync(sku);
    }
}