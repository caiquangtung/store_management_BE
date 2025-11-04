using AutoMapper;
using StoreManagement.Application.DTOs.Reports;
using StoreManagement.Domain.Interfaces;
using System.Collections.Generic; 
using System.Linq; 
using System;
using System.Threading.Tasks;

namespace StoreManagement.Application.Services;

public class ReportService : IReportService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly IInventoryAdjustmentRepository _adjustmentRepository;
    

    public ReportService(IOrderRepository orderRepository,IPurchaseRepository purchaseRepository,
        IInventoryAdjustmentRepository adjustmentRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _purchaseRepository = purchaseRepository;
        _adjustmentRepository = adjustmentRepository;
    }

    public async Task<IEnumerable<SalesSummaryResponse>> GetSalesOverviewAsync(DateTime startDate, DateTime endDate, string groupBy)
    {
        var rawData = await _orderRepository.GetSalesOverviewAsync(startDate, endDate, groupBy);
        
        return rawData.Select(d => new SalesSummaryResponse
        {
            Period = d.Period,
            TotalRevenue = d.TotalRevenue,
            NumberOfOrders = d.NumberOfOrders,
            AverageOrderValue = (d.NumberOfOrders > 0) ? d.TotalRevenue / d.NumberOfOrders : 0
        });
    }

    public async Task<IEnumerable<DeadStockProductResponse>> GetDeadStockProductsAsync(DateTime startDate, DateTime endDate)
    {
        var deadStockProducts = await _productRepository.GetDeadStockProductsAsync(startDate, endDate);

        return deadStockProducts.Select(p => new DeadStockProductResponse
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            Barcode = p.Barcode,
            Price = p.Price,
            QuantityInStock = p.Inventory.FirstOrDefault()?.Quantity ?? 0
        });
    }
    
    private class LedgerMovement
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public int Change { get; set; }
        public string Reference { get; set; } = string.Empty;
    }

    // [THÊM MỚI] Logic Sổ Kho
    public async Task<InventoryLedgerResponse> GetInventoryLedgerAsync(int productId, DateTime startDate, DateTime endDate)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw new InvalidOperationException("Product not found");
        }

        // Đảm bảo startDate là 00:00:00
        startDate = startDate.Date;
        // Đảm bảo endDate là 23:59:59.999... của ngày
        var exclusiveEndDate = endDate.Date.AddDays(1);

        // --- BƯỚC 1: TÍNH TỒN ĐẦU KỲ ---
        var salesBefore = await _orderRepository.GetLedgerMovementsAsync(productId, null, startDate);
        var purchasesBefore = await _purchaseRepository.GetLedgerMovementsAsync(productId, null, startDate);
        var adjustmentsBefore = await _adjustmentRepository.GetLedgerMovementsAsync(productId, null, startDate);

        int startQuantity = 0;
        startQuantity += purchasesBefore.Sum(i => i.Quantity);
        startQuantity -= salesBefore.Sum(i => i.Quantity); // Trừ đi số lượng đã bán
        startQuantity += adjustmentsBefore.Sum(i => i.Quantity); // Cộng/trừ số lượng điều chỉnh

        // --- BƯỚC 2: LẤY GIAO DỊCH TRONG KỲ ---
        var salesInPeriod = await _orderRepository.GetLedgerMovementsAsync(productId, startDate, exclusiveEndDate);
        var purchasesInPeriod = await _purchaseRepository.GetLedgerMovementsAsync(productId, startDate, exclusiveEndDate);
        var adjustmentsInPeriod = await _adjustmentRepository.GetLedgerMovementsAsync(productId, startDate, exclusiveEndDate);

        var allMovements = new List<LedgerMovement>();

        // --- BƯỚC 3: GỘP 3 LUỒNG ---
        allMovements.AddRange(salesInPeriod.Select(i => new LedgerMovement
        {
            Date = i.Order!.OrderDate,
            Type = "Bán hàng",
            Change = -i.Quantity, // Bán hàng là TRỪ kho
            Reference = $"Order #{i.OrderId}"
        }));

        allMovements.AddRange(purchasesInPeriod.Select(i => new LedgerMovement
        {
            Date = i.Purchase!.CreatedAt,
            Type = "Nhập hàng",
            Change = i.Quantity, // Nhập hàng là CỘNG kho
            Reference = $"Purchase #{i.PurchaseId}"
        }));

        allMovements.AddRange(adjustmentsInPeriod.Select(i => new LedgerMovement
        {
            Date = i.CreatedAt,
            Type = i.Reason, // Lý do: "Hàng hỏng", "Trả hàng", ...
            Change = i.Quantity, // Điều chỉnh có thể âm hoặc dương
            Reference = $"Adj. #{i.AdjustmentId}"
        }));

        // --- BƯỚC 4: SẮP XẾP VÀ TÍNH SỐ DƯ ---
        var sortedMovements = allMovements.OrderBy(m => m.Date).ToList();

        var response = new InventoryLedgerResponse
        {
            ProductId = productId,
            ProductName = product.ProductName,
            StartDate = startDate,
            EndDate = endDate.Date,
            StartQuantity = startQuantity
        };

        int currentBalance = startQuantity;
        foreach (var movement in sortedMovements)
        {
            currentBalance += movement.Change;
            response.Movements.Add(new LedgerMovementResponse
            {
                Date = movement.Date,
                Type = movement.Type,
                Reference = movement.Reference,
                Change = movement.Change,
                Balance = currentBalance // Số dư sau giao dịch
            });
        }

        response.EndQuantity = currentBalance;
        return response;
    }
    
    public async Task<IEnumerable<PurchaseSummaryResponse>> GetPurchaseSummaryAsync(DateTime startDate, DateTime endDate, string groupBy)
    {
        var rawData = await _purchaseRepository.GetPurchaseSummaryAsync(startDate, endDate, groupBy);
        
        // Chỉ cần map đơn giản từ raw DTO sang response DTO
        return rawData.Select(d => new PurchaseSummaryResponse
        {
            Period = d.Period,
            TotalSpent = d.TotalSpent,
            NumberOfPurchases = d.NumberOfPurchases
        });
    }
}