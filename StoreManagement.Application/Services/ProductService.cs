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

    public async Task<IEnumerable<ABCProductResponse>> GetABCAnalysisAsync(DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 10)
    {
        // Get raw ABC data with date filter
        var abcData = await _productRepository.GetABCAnalysisDataAsync(fromDate, toDate);

        // Filter out products with null or zero value (no sales in period)
        var validData = abcData.Where(d => d.Value.HasValue && d.Value.Value > 0).ToList();

        if (!validData.Any())
        {
            return Enumerable.Empty<ABCProductResponse>();
        }

        // Sort by score descending
        var sortedData = validData.OrderByDescending(d => d.Score).ToList();

        // Calculate total score for percentile
        var totalScore = sortedData.Sum(d => d.Score);

        // Classify A/B/C
        var classifiedProducts = new List<ABCProductResponse>();
        var currentIndex = 0;

        // A: Top 20% by score contribution
        var aThreshold = totalScore * 0.20m;  // Decimal literal
        var aScore = 0m;
        while (currentIndex < sortedData.Count && aScore < aThreshold)
        {
            var data = sortedData[currentIndex];
            classifiedProducts.Add(new ABCProductResponse
            {
                ProductId = data.ProductId,
                ProductName = data.ProductName,
                Barcode = data.Barcode ?? string.Empty,
                Value = data.Value ?? 0m,
                Frequency = data.Frequency,
                Score = data.Score,
                ABCClassification = "A"
            });
            aScore += data.Score;
            currentIndex++;
        }

        // B: Next 30%
        var bThreshold = totalScore * 0.30m;  // Decimal literal
        var bScore = 0m;
        while (currentIndex < sortedData.Count && bScore < bThreshold)
        {
            var data = sortedData[currentIndex];
            classifiedProducts.Add(new ABCProductResponse
            {
                ProductId = data.ProductId,
                ProductName = data.ProductName,
                Barcode = data.Barcode ?? string.Empty,
                Value = data.Value ?? 0m,
                Frequency = data.Frequency,
                Score = data.Score,
                ABCClassification = "B"
            });
            bScore += data.Score;
            currentIndex++;
        }

        // C: Remaining 50%
        while (currentIndex < sortedData.Count)
        {
            var data = sortedData[currentIndex];
            classifiedProducts.Add(new ABCProductResponse
            {
                ProductId = data.ProductId,
                ProductName = data.ProductName,
                Barcode = data.Barcode ?? string.Empty,
                Value = data.Value ?? 0m,
                Frequency = data.Frequency,
                Score = data.Score,
                ABCClassification = "C"
            });
            currentIndex++;
        }

        // Apply pagination
        var totalCount = classifiedProducts.Count;
        var pagedItems = classifiedProducts
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return pagedItems;
    }
}