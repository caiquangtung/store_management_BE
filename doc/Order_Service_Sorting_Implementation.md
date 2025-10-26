# Order Service Sorting Implementation

## Tổng quan

Đã cập nhật `OrderService` để thêm tính năng sắp xếp (sorting) tương tự như `ProductService`, đảm bảo tính nhất quán trong API.

## Các thay đổi đã thực hiện

### 1. Cập nhật OrderService.GetAllPagedAsync

**File:** `StoreManagement.Application/Services/OrderService.cs`

**Thay đổi:**

- Thêm tham số `string? sortBy = null` và `bool sortDesc = false`
- Implement logic sắp xếp với whitelist các trường được phép:
  - `id` → OrderId
  - `orderdate` → OrderDate
  - `totalamount` → TotalAmount
  - `status` → Status
  - `customerid` → CustomerId
  - `userid` → UserId
- Thêm stable tie-breaker bằng OrderId để đảm bảo kết quả nhất quán
- Mặc định sắp xếp theo OrderId nếu không chỉ định hoặc trường không hợp lệ

```csharp
public async Task<(IEnumerable<OrderResponse> Items, int TotalCount)> GetAllPagedAsync(
    int pageNumber, int pageSize, OrderStatus? status = null, int? userId = null, int? customerId = null,
    string? sortBy = null, bool sortDesc = false)
{
    // Build filter expression
    System.Linq.Expressions.Expression<Func<Order, bool>>? filter = null;

    if (status.HasValue || userId.HasValue || customerId.HasValue)
    {
        filter = o =>
            (!status.HasValue || o.Status == status.Value) &&
            (!userId.HasValue || o.UserId == userId) &&
            (!customerId.HasValue || o.CustomerId == customerId);
    }

    // Build order expression with whitelist and stable tie-breaker by OrderId
    System.Linq.Expressions.Expression<Func<Order, object>> primarySort = (sortBy ?? string.Empty).ToLower() switch
    {
        "id" => o => o.OrderId,
        "orderdate" => o => o.OrderDate,
        "totalamount" => o => o.TotalAmount ?? 0,
        "status" => o => o.Status,
        "customerid" => o => o.CustomerId ?? 0,
        "userid" => o => o.UserId ?? 0,
        _ => o => o.OrderId
    };

    Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy = q =>
    {
        var ordered = sortDesc ? q.OrderByDescending(primarySort) : q.OrderBy(primarySort);
        return sortDesc ? ordered.ThenByDescending(o => o.OrderId) : ordered.ThenBy(o => o.OrderId);
    };

    var (items, totalCount) = await _orderRepository.GetPagedAsync(pageNumber, pageSize, filter, orderBy);
    var mappedItems = _mapper.Map<IEnumerable<OrderResponse>>(items);

    return (mappedItems, totalCount);
}
```

### 2. Cập nhật IOrderService Interface

**File:** `StoreManagement.Application/Services/IOrderService.cs`

**Thay đổi:**

- Cập nhật signature của method `GetAllPagedAsync` để bao gồm các tham số sorting mới

```csharp
Task<(IEnumerable<OrderResponse> Items, int TotalCount)> GetAllPagedAsync(
    int pageNumber, int pageSize, OrderStatus? status = null, int? userId = null, int? customerId = null,
    string? sortBy = null, bool sortDesc = false);
```

### 3. Cập nhật OrderController

**File:** `StoreManagement.API/Controllers/OrderController.cs`

**Thay đổi:**

- Thêm tham số `[FromQuery] string? sortBy = null` và `[FromQuery] bool sortDesc = false`
- Cập nhật lời gọi service để truyền các tham số sorting
- Cập nhật documentation comment

```csharp
/// <summary>
/// Get paginated list of orders with filters and sorting
/// </summary>
[HttpGet]
public async Task<IActionResult> GetOrders(
    [FromQuery] PaginationParameters pagination,
    [FromQuery] OrderStatus? status = null,
    [FromQuery] int? userId = null,
    [FromQuery] int? customerId = null,
    [FromQuery] string? sortBy = null,
    [FromQuery] bool sortDesc = false)
{
    try
    {
        var (orders, totalCount) = await _orderService.GetAllPagedAsync(
            pagination.PageNumber, pagination.PageSize, status, userId, customerId, sortBy, sortDesc);

        var pagedResult = PagedResult<OrderResponse>.Create(orders, totalCount, pagination.PageNumber, pagination.PageSize);
        return Ok(ApiResponse<PagedResult<OrderResponse>>.SuccessResponse(pagedResult, "Orders retrieved successfully"));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error occurred while retrieving orders");
        return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving orders"));
    }
}
```

## Các trường có thể sắp xếp

| Tham số sortBy | Trường Order | Mô tả                  |
| -------------- | ------------ | ---------------------- |
| `id`           | OrderId      | ID đơn hàng (mặc định) |
| `orderdate`    | OrderDate    | Ngày tạo đơn hàng      |
| `totalamount`  | TotalAmount  | Tổng tiền đơn hàng     |
| `status`       | Status       | Trạng thái đơn hàng    |
| `customerid`   | CustomerId   | ID khách hàng          |
| `userid`       | UserId       | ID người dùng          |

## Ví dụ sử dụng API

### Sắp xếp theo ngày tạo đơn hàng (mới nhất trước)

```
GET /api/orders?pageNumber=1&pageSize=10&sortBy=orderdate&sortDesc=true
```

### Sắp xếp theo tổng tiền (cao nhất trước)

```
GET /api/orders?pageNumber=1&pageSize=10&sortBy=totalamount&sortDesc=true
```

### Kết hợp lọc và sắp xếp

```
GET /api/orders?pageNumber=1&pageSize=10&status=Pending&sortBy=orderdate&sortDesc=true
```

## Tính năng bảo mật

- **Whitelist Sorting**: Chỉ cho phép sắp xếp theo các trường được định nghĩa trước, ngăn chặn SQL injection
- **Stable Sorting**: Sử dụng OrderId làm tie-breaker để đảm bảo kết quả nhất quán
- **Default Behavior**: Mặc định sắp xếp theo OrderId nếu không chỉ định hoặc trường không hợp lệ

## File Test

Đã tạo file `Order_Sorting_Tests.http` với 17 test cases để kiểm tra đầy đủ tính năng sorting.

## Lợi ích

1. **Tính nhất quán**: OrderService giờ đây có cùng tính năng sorting như ProductService
2. **Trải nghiệm người dùng**: Cho phép sắp xếp dữ liệu theo nhiều tiêu chí khác nhau
3. **Hiệu suất**: Sắp xếp được thực hiện ở database level, không phải application level
4. **Bảo mật**: Sử dụng whitelist để ngăn chặn các cuộc tấn công injection
5. **Ổn định**: Stable sorting đảm bảo kết quả nhất quán qua các lần gọi API
