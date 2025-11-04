using AutoMapper;
using StoreManagement.Application.DTOs.Purchase;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Enums;
using StoreManagement.Domain.Interfaces;
using System.Linq.Expressions;

namespace StoreManagement.Application.Services;

public class PurchaseService : IPurchaseService
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IMapper _mapper;

    public PurchaseService(IPurchaseRepository purchaseRepository, IInventoryRepository inventoryRepository, IMapper mapper)
    {
        _purchaseRepository = purchaseRepository;
        _inventoryRepository = inventoryRepository;
        _mapper = mapper;
    }

    public async Task<PurchaseResponse> CreatePurchaseAsync(CreatePurchaseRequest request, int userId)
    {
        var purchase = _mapper.Map<Purchase>(request);
        purchase.UserId = userId;
        purchase.Status = PurchaseStatus.Pending;
        
        // Tính tổng tiền
        purchase.TotalAmount = request.Items.Sum(item => item.Quantity * item.PurchasePrice);

        var createdPurchase = await _purchaseRepository.AddAsync(purchase);
        await _purchaseRepository.SaveChangesAsync(); // Lưu để lấy PurchaseId và PurchaseItemIds

        // Tải lại với chi tiết để trả về
        var result = await _purchaseRepository.GetByIdWithDetailsAsync(createdPurchase.PurchaseId);
        return _mapper.Map<PurchaseResponse>(result!);
    }

    public async Task<PurchaseResponse> ConfirmPurchaseAsync(int purchaseId)
    {
        var purchase = await _purchaseRepository.GetByIdWithDetailsAsync(purchaseId);
        if (purchase == null)
            throw new InvalidOperationException("Purchase not found");
        if (purchase.Status != PurchaseStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm purchase with status '{purchase.Status}'");

        // *** LOGIC CỐT LÕI: CỘNG HÀNG VÀO KHO ***
        foreach (var item in purchase.PurchaseItems)
        {
            var inventory = await _inventoryRepository.GetByProductIdAsync(item.ProductId);
            if (inventory == null)
            {
                // Nếu sản phẩm chưa có trong kho, tạo mới
                inventory = new Inventory
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                };
                await _inventoryRepository.AddAsync(inventory);
            }
            else
            {
                // Nếu có rồi, cộng dồn
                inventory.Quantity += item.Quantity;
                await _inventoryRepository.UpdateAsync(inventory);
            }
        }

        // Cập nhật trạng thái đơn nhập hàng
        purchase.Status = PurchaseStatus.Confirmed;
        purchase.UpdatedAt = DateTime.UtcNow;
        await _purchaseRepository.UpdateAsync(purchase);

        // Lưu tất cả thay đổi (cập nhật kho và cập nhật đơn hàng) TRONG CÙNG 1 GIAO DỊCH
        await _purchaseRepository.SaveChangesAsync();

        return _mapper.Map<PurchaseResponse>(purchase);
    }

    public async Task<PurchaseResponse> CancelPurchaseAsync(int purchaseId)
    {
        var purchase = await _purchaseRepository.GetByIdAsync(purchaseId);
        if (purchase == null)
            throw new InvalidOperationException("Purchase not found");
        if (purchase.Status != PurchaseStatus.Pending)
            throw new InvalidOperationException($"Cannot cancel purchase with status '{purchase.Status}'");
        
        // Chỉ cần cập nhật status. Không cần hoàn kho vì hàng chưa bao giờ được cộng.
        purchase.Status = PurchaseStatus.Canceled;
        purchase.UpdatedAt = DateTime.UtcNow;
        await _purchaseRepository.UpdateAsync(purchase);
        await _purchaseRepository.SaveChangesAsync();

        return _mapper.Map<PurchaseResponse>(purchase);
    }

    public async Task<PurchaseResponse?> GetPurchaseByIdAsync(int purchaseId)
    {
        var purchase = await _purchaseRepository.GetByIdWithDetailsAsync(purchaseId);
        return _mapper.Map<PurchaseResponse>(purchase);
    }

    public async Task<(IEnumerable<PurchaseResponse> Items, int TotalCount)> GetAllPurchasesPagedAsync(
        int pageNumber, 
        int pageSize, 
        PurchaseStatus? status = null, 
        int? supplierId = null, 
        string? searchTerm = null, 
        string? sortBy = null, 
        bool sortDesc = false)
    {
        // 1. Xây dựng Biểu thức Lọc (Filter Expression)
        Expression<Func<Purchase, bool>> filter = p =>
            (!status.HasValue || p.Status == status.Value) &&
            (!supplierId.HasValue || p.SupplierId == supplierId.Value) &&
            (string.IsNullOrEmpty(searchTerm) ||
                (p.Notes != null && p.Notes.Contains(searchTerm)) ||
                (p.Supplier != null && p.Supplier.Name.Contains(searchTerm)) ||
                (p.User != null && p.User.FullName != null && p.User.FullName.Contains(searchTerm))
            );

        // 2. Xây dựng Biểu thức Sắp xếp (Sort Expression)
        Expression<Func<Purchase, object>> primarySort = (sortBy ?? string.Empty).ToLower() switch
        {
            "id" => p => p.PurchaseId,
            "supplier" => p => p.Supplier != null ? p.Supplier.Name : string.Empty,
            "user" => p => p.User != null ? p.User.FullName ?? string.Empty : string.Empty,
            "status" => p => p.Status,
            "total" or "totalamount" => p => p.TotalAmount,
            _ => p => p.CreatedAt
        };

        Func<IQueryable<Purchase>, IOrderedQueryable<Purchase>> orderBy = q =>
        {
            var ordered = sortDesc ? q.OrderByDescending(primarySort) : q.OrderBy(primarySort);
            
            // Thêm sắp xếp phụ để đảm bảo phân trang ổn định
            if (sortBy != "id")
                ordered = sortDesc ? ordered.ThenByDescending(p => p.PurchaseId) : ordered.ThenBy(p => p.PurchaseId);
                
            return ordered;
        };

        // 3. Gọi Repository (đã override)
        var (items, totalCount) = await _purchaseRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter,
            orderBy);

        // 4. Ánh xạ sang Response DTO
        return (_mapper.Map<IEnumerable<PurchaseResponse>>(items), totalCount);
    }
}