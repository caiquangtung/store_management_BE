using AutoMapper;
using StoreManagement.Application.DTOs.InventoryAdjustment;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Interfaces;
using System.Linq.Expressions;

namespace StoreManagement.Application.Services;

public class InventoryAdjustmentService : IInventoryAdjustmentService
{
    private readonly IInventoryAdjustmentRepository _adjustmentRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IMapper _mapper;

    public InventoryAdjustmentService(IInventoryAdjustmentRepository adjustmentRepository, IInventoryRepository inventoryRepository, IMapper mapper)
    {
        _adjustmentRepository = adjustmentRepository;
        _inventoryRepository = inventoryRepository;
        _mapper = mapper;
    }

    public async Task<AdjustmentResponse> CreateAdjustmentAsync(CreateAdjustmentRequest request, int userId)
    {
        // *** LOGIC CỐT LÕI: ĐIỀU CHỈNH KHO ***
        var inventory = await _inventoryRepository.GetByProductIdAsync(request.ProductId);

        if (inventory == null)
        {
            if (request.Quantity < 0)
                throw new InvalidOperationException("Cannot make negative adjustment to non-existent inventory");
            
            // Nếu chưa có, tạo mới
            inventory = new Inventory
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity
            };
            await _inventoryRepository.AddAsync(inventory);
        }
        else
        {
            // Kiểm tra tồn kho
            if (inventory.Quantity + request.Quantity < 0)
                throw new InvalidOperationException($"Insufficient stock. Current: {inventory.Quantity}, Trying to adjust by: {request.Quantity}");

            // Cập nhật kho
            inventory.Quantity += request.Quantity;
            await _inventoryRepository.UpdateAsync(inventory);
        }

        // Ghi lại log điều chỉnh
        var adjustment = _mapper.Map<InventoryAdjustment>(request);
        adjustment.UserId = userId;
        await _adjustmentRepository.AddAsync(adjustment);

        // Lưu cả 2 thay đổi (kho và log) trong 1 giao dịch
        await _adjustmentRepository.SaveChangesAsync();

        // Tải lại để lấy thông tin User, Product
        var createdAdjustment = await _adjustmentRepository.GetByIdAsync(adjustment.AdjustmentId);
        // (Cần tối ưu: GetByIdWithDetailsAsync)
        return _mapper.Map<AdjustmentResponse>(createdAdjustment); 
    }

    public async Task<(IEnumerable<AdjustmentResponse> Items, int TotalCount)> GetAllAdjustmentsPagedAsync(int pageNumber, int pageSize, int? productId = null, string? sortBy = null, bool sortDesc = false)
    {
        Expression<Func<InventoryAdjustment, bool>> filter = a =>
            (!productId.HasValue || a.ProductId == productId.Value);

        // (Thêm logic sắp xếp nếu cần) ...

        var (items, totalCount) = await _adjustmentRepository.GetPagedAsync(pageNumber, pageSize, filter);
        return (_mapper.Map<IEnumerable<AdjustmentResponse>>(items), totalCount);
    }
}