# Triển Khai API Quản Lý Tồn Kho (Inventory API)

## Tổng Quan

Tài liệu này mô tả triển khai API Quản Lý Tồn Kho (Inventory) cho hệ thống Store Management. API Inventory cung cấp đầy đủ các thao tác CRUD để quản lý thông tin tồn kho, bao gồm join với sản phẩm và cảnh báo tồn kho thấp (low stock).

## Kiến Trúc

### Các Lớp

- **Lớp Domain**: Entity Inventory và giao diện ICustomerRepository.
- **Lớp Application**: DTOs, dịch vụ, validator và profile AutoMapper.
- **Lớp Infrastructure**: Triển khai InventoryRepository và cấu hình cơ sở dữ liệu.
- **Lớp API**: InventoryController với các endpoint REST.

## Chi Tiết Triển Khai

### 1. Lớp Domain

#### Entity Inventory

```csharp
public class Inventory
{
    public int InventoryId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
}
```

#### Giao Diện ICustomerRepository

```csharp
public interface IInventoryRepository : IRepository<Inventory>
{
    Task<IEnumerable<Inventory>> GetAllWithProductAsync();
    Task<Inventory?> GetByIdWithProductAsync(int id);
    Task<IEnumerable<Inventory>> GetLowStockAsync(int threshold);
}
```

### 2. Lớp Application

#### DTOs

- **CreateInventoryRequest**: Để tạo entry tồn kho mới.
- **UpdateInventoryRequest**: Để cập nhật tồn kho hiện có.
- **InventoryResponse**: Để trả về thông tin tồn kho (join với sản phẩm).
- **LowStockResponse**: Để trả về danh sách tồn kho thấp.

#### Validators

- **UpdateInventoryRequestValidator**: Xác thực dữ liệu cập nhật tồn kho.

#### Dịch Vụ

- **IInventoryService**: Giao diện định nghĩa các thao tác tồn kho.
- **InventoryService**: Triển khai logic nghiệp vụ tồn kho.

#### AutoMapper Profile

- **InventoryMappingProfile**: Ánh xạ giữa entity Inventory và DTOs.

### 3. Lớp Infrastructure

#### InventoryRepository

```csharp
public class InventoryRepository : BaseRepository<Inventory>, IInventoryRepository
{
    public async Task<IEnumerable<Inventory>> GetAllWithProductAsync()
    {
        return await _dbSet
            .Include(i => i.Product)
                .ThenInclude(p => p.Category)
            .Include(i => i.Product)
                .ThenInclude(p => p.Supplier)
            .OrderBy(i => i.Product.ProductName)
            .ToListAsync();
    }

    public async Task<Inventory?> GetByIdWithProductAsync(int id)
    {
        return await _dbSet
            .Include(i => i.Product)
                .ThenInclude(p => p.Category)
            .Include(i => i.Product)
                .ThenInclude(p => p.Supplier)
            .FirstOrDefaultAsync(i => i.InventoryId == id);
    }

    public async Task<IEnumerable<Inventory>> GetLowStockAsync(int threshold)
    {
        return await _dbSet
            .Include(i => i.Product)
                .ThenInclude(p => p.Category)
            .Include(i => i.Product)
                .ThenInclude(p => p.Supplier)
            .Where(i => i.Quantity < threshold && i.Quantity > 0)
            .OrderBy(i => i.Quantity)
            .ToListAsync();
    }
}
```

#### Cấu Hình Cơ Sở Dữ Liệu

Entity Inventory được cấu hình trong StoreDbContext với ánh xạ cột phù hợp (product_id, quantity, updated_at) và mối quan hệ foreign key với Product.

### 4. Lớp API

#### Các Endpoint Của InventoryController

##### GET /api/inventory

- **Mô Tả**: Lấy danh sách tồn kho với join sản phẩm và phân trang.
- **Phân Quyền**: Staff, Admin.
- **Tham Số**:
  - `pageNumber` (tùy chọn, mặc định: 1)
  - `pageSize` (tùy chọn, mặc định: 10, tối đa: 100)
- **Phản Hồi**: PagedResult<InventoryResponse>

##### GET /api/inventory/{id}

- **Mô Tả**: Lấy tồn kho theo ID với join sản phẩm.
- **Phân Quyền**: Staff, Admin.
- **Tham Số**: `id` (ID tồn kho).
- **Phản Hồi**: InventoryResponse

##### POST /api/inventory

- **Mô Tả**: Tạo entry tồn kho mới.
- **Phân Quyền**: Admin only.
- **Body**: CreateInventoryRequest
- **Phản Hồi**: InventoryResponse (201 Created)

##### PUT /api/inventory/{id}

- **Mô Tả**: Cập nhật số lượng tồn kho.
- **Phân Quyền**: Admin only.
- **Tham Số**: `id` (ID tồn kho).
- **Body**: UpdateInventoryRequest
- **Phản Hồi**: InventoryResponse

##### PUT /api/inventory/{id}/set-zero

- **Mô Tả**: Đặt số lượng tồn kho về 0 (thay thế DELETE).
- **Phân Quyền**: Admin only.
- **Tham Số**: `id` (ID tồn kho).
- **Phản Hồi**: boolean (200 OK nếu thành công)

##### GET /api/inventory/low-stock

- **Mô Tả**: Lấy danh sách tồn kho thấp (low stock) với ngưỡng threshold.
- **Phân Quyền**: Staff, Admin.
- **Tham Số**:
  - `threshold` (tùy chọn, mặc định: 10) – Ngưỡng cảnh báo.
- **Phản Hồi**: IEnumerable<LowStockResponse>

## Các Tính Năng Chính

### 1. Phân Trang (Pagination)

- Xử lý phân trang ở mức controller.
- Dịch vụ trả về tất cả dữ liệu phù hợp, controller áp dụng phân trang.
- Hỗ trợ tham số pageNumber và pageSize.
- Giới hạn tối đa pageSize = 100 mục.

### 2. Join Với Sản Phẩm

- Tự động join với bảng `products` để hiển thị thông tin đầy đủ (tên sản phẩm, mã vạch, giá).
- Eager loading với Category và Supplier để tránh N+1 query.

### 3. Cảnh Báo Tồn Kho Thấp (Low Stock Alert)

- Query quantity < threshold (mặc định 10).
- Tính toán gợi ý số lượng cần nhập (reorderQuantity = threshold - quantity).
- Chỉ hiển thị sản phẩm có quantity > 0 (loại trừ hết hàng).

### 4. Phân Quyền

- Kiểm soát quyền truy cập dựa trên role (AdminOrStaff, AdminOnly).
- Sử dụng JWT Bearer Token cho authentication.

### 5. Xử Lý Lỗi

- Xử lý ngoại lệ toàn diện.
- Trả về mã HTTP phù hợp và thông báo lỗi rõ ràng.

## Schema Cơ Sở Dữ Liệu

### Bảng Inventory

```sql
CREATE TABLE inventory (
    inventory_id INT AUTO_INCREMENT PRIMARY KEY,
    product_id INT NOT NULL,
    quantity INT DEFAULT 0,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE CASCADE
);
```

### Mối Quan Hệ

- Inventory có một Product (one-to-one hoặc one-to-many tùy sản phẩm).
- Product có nhiều Inventory (nếu hỗ trợ nhiều kho, nhưng schema hiện tại là 1:1).
- Tích hợp với Orders qua Product (gián tiếp).

## Quy Tắc Validation

### CreateInventoryRequest

- ProductId: Bắt buộc, >0 (phải tồn tại trong products).
- Quantity: Bắt buộc, >=0.

### UpdateInventoryRequest

- Quantity: Bắt buộc, >=0 (không âm).
- SetToZero: Tùy chọn, boolean (nếu true, set quantity = 0).

### LowStock (Query)

- Threshold: >0 (mặc định 10).

## Đăng Ký Dịch Vụ

Các dịch vụ sau được đăng ký trong DI container:

- `IInventoryRepository` → `InventoryRepository`
- `IInventoryService` → `InventoryService`
- Validator cho UpdateInventoryRequest.
- InventoryMappingProfile cho AutoMapper.

## Các Lưu Ý Test

### Unit Tests

- Test logic dịch vụ InventoryService.
- Test truy vấn repository InventoryRepository.
- Test endpoint InventoryController.
- Test validator.

### Integration Tests

- Test luồng API hoàn chỉnh.
- Test thao tác cơ sở dữ liệu.
- Test chính sách phân quyền.

## Các Cải Tiến Tương Lai

1. **Tích Hợp Với Đơn Hàng**: Tự động cập nhật quantity khi tạo order (giảm khi bán).
2. **Nhập/Xuất Kho**: Endpoint cho nhập hàng (tăng quantity) và xuất kho (giảm).
3. **Báo Cáo Tồn Kho**: Thêm filter theo ngày, kho hàng (nếu mở rộng).
4. **Cảnh Báo Email**: Gửi thông báo low stock qua email/SMS.
5. **Audit Trail**: Theo dõi lịch sử thay đổi quantity.
6. **Hỗ Trợ Đa Kho**: Mở rộng cho nhiều kho hàng.

## Cấu Hình

### Connection String

Đảm bảo chuỗi kết nối được cấu hình đúng trong `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=store_management;Uid=root;Pwd=securepassword123;"
  }
}
```

### JWT Settings

Endpoint Inventory yêu cầu xác thực JWT. Đảm bảo cấu hình JWT:

```json
{
  "JwtSettings": {
    "Secret": "your-secret-key",
    "Issuer": "StoreManagementAPI",
    "Audience": "StoreManagementClient",
    "ExpireMinutes": 1440
  }
}
```

## Ví Dụ Sử Dụng

### Tạo Tồn Kho Mới

```bash
POST /api/inventory
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "productId": 1,
  "quantity": 50
}
```

### Lấy Danh Sách Tồn Kho Với Phân Trang

```bash
GET /api/inventory?pageNumber=1&pageSize=10
Authorization: Bearer <jwt-token>
```

### Cập Nhật Tồn Kho

```bash
PUT /api/inventory/1
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "quantity": 30
}
```

### Đặt Tồn Kho Về 0

```bash
PUT /api/inventory/1/set-zero
Authorization: Bearer <jwt-token>
```

### Lấy Tồn Kho Thấp

```bash
GET /api/inventory/low-stock?threshold=10
Authorization: Bearer <jwt-token>
```

## Kết Luận

Triển khai API Quản Lý Tồn Kho tuân thủ nguyên tắc Clean Architecture với sự phân tách trách nhiệm rõ ràng, validation toàn diện, phân quyền dựa trên role, và xử lý lỗi mạnh mẽ. Phân trang được xử lý ở mức controller để tối ưu hiệu suất và linh hoạt. API hỗ trợ join hiệu quả với sản phẩm và cảnh báo tồn kho thấp, phù hợp cho hệ thống quản lý bán lẻ.