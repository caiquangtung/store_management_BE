using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using StoreManagement.Application.DTOs.Products;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;
using System.Linq.Expressions;

namespace StoreManagement.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _environment;  // For file upload

    public ProductService(IProductRepository productRepository, IMapper mapper, IWebHostEnvironment environment)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _environment = environment;
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

    public async Task<(IEnumerable<ProductResponse> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize)
    {
        // Get paged data from repository with ordering by name
        var (items, totalCount) = await _productRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            null, // no filter
            query => query.OrderBy(p => p.Name));

        // Map to response DTOs
        var mappedItems = _mapper.Map<IEnumerable<ProductResponse>>(items);

        return (mappedItems, totalCount);
    }

    public async Task<ProductResponse?> CreateAsync(CreateProductRequest request)
    {
        // Check if SKU already exists
        if (!string.IsNullOrEmpty(request.Barcode) && await _productRepository.SKUExistsAsync(request.Barcode))
        {
            throw new InvalidOperationException("SKU already exists");
        }

        // Handle image upload
        string? imagePath = null;
        if (request.Image != null)
        {
            imagePath = await SaveImageAsync(request.Image);
        }

        // Map DTO to entity
        var product = _mapper.Map<Product>(request);
        product.CreatedAt = DateTime.UtcNow;
        product.ImagePath = imagePath;  // Set image path

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

        // Handle image update
        if (request.Image != null)
        {
            // Delete old image if exists
            if (!string.IsNullOrEmpty(product.ImagePath))
            {
                await DeleteImageAsync(product.ImagePath);
            }
            product.ImagePath = await SaveImageAsync(request.Image);
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

        // Delete image file if exists
        if (!string.IsNullOrEmpty(product.ImagePath))
        {
            await DeleteImageAsync(product.ImagePath);
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

    private async Task<string?> SaveImageAsync(IFormFile image)
    {
        if (image == null || image.Length == 0)
        {
            return null;
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Only JPG, JPEG, PNG images are allowed.");
        }

        if (image.Length > 5 * 1024 * 1024)  // 5MB limit
        {
            throw new InvalidOperationException("Image size must be less than 5MB.");
        }

        var uploadsDir = Path.Combine(_environment.WebRootPath, "images/products");
        if (!Directory.Exists(uploadsDir))
        {
            Directory.CreateDirectory(uploadsDir);
        }

        var fileName = Guid.NewGuid() + extension;
        var filePath = Path.Combine(uploadsDir, fileName);
        var relativePath = $"/images/products/{fileName}";

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }

        return relativePath;
    }

    private async Task DeleteImageAsync(string imagePath)
    {
        var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}