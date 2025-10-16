# Triển Khai API Quản Lý Sản Phẩm (Product API)

## Tổng Quan

Tài liệu này mô tả triển khai API Quản Lý Sản Phẩm (Product) cho hệ thống Store Management. API Product cung cấp đầy đủ các thao tác CRUD để quản lý thông tin sản phẩm, bao gồm upload hình ảnh, liên kết với danh mục và nhà cung cấp, kiểm tra mã vạch (SKU) duy nhất, và phân tích ABC để phân loại sản phẩm dựa trên doanh thu và tần suất bán.

## Kiến Trúc

### Các Lớp

- **Lớp Domain**: Entity Product và giao diện IProductRepository.
- **Lớp Application**: DTOs, dịch vụ, validator và profile AutoMapper.
- **Lớp Infrastructure**: ProductRepository triển khai và cấu hình cơ sở dữ liệu.
- **Lớp API**: ProductsController với các endpoint REST.

## Chi Tiết Triển Khai

### 1. Lớp Domain

#### Entity Product

```csharp
public class Product
{
    public int ProductId { get; set; }
    public int? CategoryId { get; set; }
    public int? SupplierId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal Price { get; set; }
    public string Unit { get; set; } = "pcs";
    public DateTime CreatedAt { get; set; }
    public string? ImagePath { get; set; } = null;

    // Navigation properties
    public virtual Category? Category { get; set; }
    public virtual Supplier? Supplier { get; set; }
    public virtual ICollection<Inventory> Inventory { get; set; } = new List<Inventory>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
```

#### Giao Diện IProductRepository

```csharp
public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
    Task<IEnumerable<Product>> GetBySupplierAsync(int supplierId);
    Task<Product?> GetBySKUAsync(string sku);
    Task<bool> SKUExistsAsync(string sku);
    Task<IEnumerable<ABCData>> GetABCAnalysisDataAsync(DateTime? fromDate = null, DateTime? toDate = null);
}
```

### 2. Lớp Application

#### DTOs

- **CreateProductRequest**: Để tạo sản phẩm mới (bao gồm upload hình ảnh qua IFormFile).
- **UpdateProductRequest**: Để cập nhật sản phẩm hiện có (hỗ trợ upload hình ảnh mới).
- **ProductResponse**: Để trả về thông tin sản phẩm (bao gồm đường dẫn hình ảnh).
- **ABCProductResponse**: Để trả về kết quả phân tích ABC (với phân loại A/B/C, value, frequency, score).

#### Validators

- **CreateProductRequestValidator**: Xác thực dữ liệu tạo sản phẩm (tên bắt buộc, giá >0, hình ảnh JPG/PNG <5MB).
- **UpdateProductRequestValidator**: Xác thực dữ liệu cập nhật sản phẩm.

#### Dịch Vụ

- **IProductService**: Giao diện định nghĩa các thao tác sản phẩm, bao gồm ABC analysis.
- **ProductService**: Triển khai logic nghiệp vụ sản phẩm (CRUD, kiểm tra SKU, upload hình ảnh, phân tích ABC).

#### AutoMapper Profile

- **ProductMappingProfile**: Ánh xạ giữa entity Product và DTOs (bao gồm ImagePath và ABCProductResponse).

### 3. Lớp Infrastructure

#### ProductRepository

```csharp
public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Where(p => p.CategoryId == categoryId)
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetBySupplierAsync(int supplierId)
    {
        return await _dbSet
            .Where(p => p.SupplierId == supplierId)
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .ToListAsync();
    }

    public async Task<Product?> GetBySKUAsync(string sku)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Barcode == sku);
    }

    public async Task<bool> SKUExistsAsync(string sku)
    {
        return await _dbSet.AnyAsync(p => p.Barcode == sku);
    }

    public async Task<IEnumerable<ABCData>> GetABCAnalysisDataAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = from oi in _context.OrderItems
                    join o in _context.Orders on oi.OrderId equals o.OrderId
                    join p in _context.Products on oi.ProductId equals p.ProductId
                    where (fromDate == null || o.OrderDate >= fromDate.Value) && (toDate == null || o.OrderDate <= toDate.Value)
                    group new { oi, p } by p.ProductId into g
                    select new ABCData
                    {
                        ProductId = g.Key,
                        ProductName = g.First().p.ProductName,
                        Barcode = g.First().p.Barcode,
                        Value = g.Sum(x => x.oi.Price * x.oi.Quantity),
                        Frequency = g.Select(x => x.oi.OrderId).Distinct().Count(),
                        Score = (decimal)(g.Sum(x => x.oi.Price * x.oi.Quantity) * g.Select(x => x.oi.OrderId).Distinct().Count())
                    };

        return await query.ToListAsync();
    }
}
```

#### Cấu Hình Cơ Sở Dữ Liệu

Entity Product được cấu hình trong StoreDbContext với ánh xạ cột phù hợp (product_id, category_id, supplier_id, barcode, price, image_path) và mối quan hệ foreign key với Category và Supplier.

### 4. Lớp API

#### Các Endpoint Của ProductsController

##### GET /api/products

- **Mô Tả**: Lấy danh sách sản phẩm với phân trang.
- **Phân Quyền**: Staff, Admin.
- **Tham Số**:
  - `pageNumber` (tùy chọn, mặc định: 1)
  - `pageSize` (tùy chọn, mặc định: 10, tối đa: 100)
- **Phản Hồi**: PagedResult<ProductResponse>

##### GET /api/products/{id}

- **Mô Tả**: Lấy sản phẩm theo ID.
- **Phân Quyền**: Staff, Admin.
- **Tham Số**: `id` (ID sản phẩm).
- **Phản Hồi**: ProductResponse

##### POST /api/products

- **Mô Tả**: Tạo sản phẩm mới (hỗ trợ upload hình ảnh qua form-data).
- **Phân Quyền**: Admin only.
- **Body**: CreateProductRequest (multipart/form-data với Image file).
- **Phản Hồi**: ProductResponse (201 Created)

##### PUT /api/products/{id}

- **Mô Tả**: Cập nhật sản phẩm hiện có (hỗ trợ upload hình ảnh mới).
- **Phân Quyền**: Admin only.
- **Tham Số**: `id` (ID sản phẩm).
- **Body**: UpdateProductRequest (multipart/form-data với Image file tùy chọn).
- **Phản Hồi**: ProductResponse

##### DELETE /api/products/{id}

- **Mô Tả**: Xóa sản phẩm.
- **Phân Quyền**: Admin only.
- **Tham Số**: `id` (ID sản phẩm).
- **Phản Hồi**: boolean

##### GET /api/products/abc-analysis

- **Mô Tả**: Phân tích ABC sản phẩm dựa trên doanh thu và tần suất bán (hỗ trợ filter thời gian).
- **Phân Quyền**: Staff, Admin.
- **Tham Số**:
  - `fromDate` (tùy chọn, YYYY-MM-DD): Ngày bắt đầu filter.
  - `toDate` (tùy chọn, YYYY-MM-DD): Ngày kết thúc filter.
  - `pageNumber` (tùy chọn, mặc định: 1)
  - `pageSize` (tùy chọn, mặc định: 10, tối đa: 100)
- **Phản Hồi**: PagedResult<ABCProductResponse>

## Các Tính Năng Chính

### 1. Phân Trang (Pagination)

- Xử lý phân trang ở mức controller.
- Dịch vụ trả về tất cả dữ liệu phù hợp, controller áp dụng phân trang.
- Hỗ trợ tham số pageNumber và pageSize.
- Giới hạn tối đa pageSize = 100 mục.

### 2. Upload Hình Ảnh

- Hỗ trợ upload file JPG/PNG <5MB qua IFormFile.
- Lưu ảnh vào thư mục `wwwroot/images/products/` với tên unique (GUID).
- Lưu đường dẫn tương đối (`/images/products/filename.jpg`) vào cột `image_path`.
- Tự động xóa ảnh cũ khi update hoặc delete sản phẩm.
- Phục vụ ảnh qua `UseStaticFiles()` (truy cập trực tiếp URL).

### 3. Kiểm Tra Mã Vạch (SKU)

- Kiểm tra barcode duy nhất trước khi tạo/cập nhật.
- Phương thức `SKUExistsAsync` để validate.

### 4. Liên Kết Với Danh Mục Và Nhà Cung Cấp

- Join eager loading với Category và Supplier khi lấy sản phẩm.
- Validate CategoryId và SupplierId >0 khi tạo/cập nhật.

### 5. Phân Tích ABC

- Query `order_items` JOIN `orders` để tính frequency (số đơn hàng), value (tổng doanh thu), score = value * frequency.
- Filter theo thời gian (fromDate/toDate) để phân tích theo kỳ.
- Phân loại A (top 20% score), B (30% tiếp), C (50% còn lại).
- Null-safe: Sử dụng `?? 0m` cho value, cast decimal trước nhân.

### 6. Phân Quyền

- Kiểm soát quyền truy cập dựa trên role (AdminOrStaff cho GET/ABC, AdminOnly cho POST/PUT/DELETE).
- Sử dụng JWT Bearer Token cho xác thực.

### 7. Xử Lý Lỗi

- Xử lý ngoại lệ toàn diện với GlobalExceptionMiddleware.
- Trả về mã HTTP phù hợp và thông báo lỗi rõ ràng.

## Schema Cơ Sở Dữ Liệu

### Bảng Products

```sql
CREATE TABLE products (
    product_id INT AUTO_INCREMENT PRIMARY KEY,
    category_id INT,
    supplier_id INT,
    product_name VARCHAR(100) NOT NULL,
    barcode VARCHAR(50) UNIQUE,
    price DECIMAL(10,2) NOT NULL,
    unit VARCHAR(20) DEFAULT 'pcs',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    image_path VARCHAR(255)
);
```

### Mối Quan Hệ

- Product có một Category (foreign key category_id).
- Product có một Supplier (foreign key supplier_id).
- Product có một Inventory (one-to-one).
- Product có nhiều OrderItem (one-to-many).

## Quy Tắc Validation

### CreateProductRequest

- ProductName: Bắt buộc, tối đa 100 ký tự.
- Price: Bắt buộc, >0.
- Unit: Bắt buộc, tối đa 20 ký tự.
- Barcode: Tùy chọn, tối đa 50 ký tự, duy nhất.
- CategoryId: Tùy chọn, >0 nếu có.
- SupplierId: Tùy chọn, >0 nếu có.
- Image: Tùy chọn, JPG/PNG, <5MB.

### UpdateProductRequest

- Tương tự CreateProductRequest, nhưng tất cả trường tùy chọn.

### ABC Analysis Params

- fromDate/toDate: Định dạng YYYY-MM-DD, fromDate <= toDate.
- pageNumber: >0.
- pageSize: 1-100.

## Đăng Ký Dịch Vụ

Các dịch vụ sau được đăng ký trong DI container:

- `IProductRepository` → `ProductRepository`
- `IProductService` → `ProductService`
- Validator cho CreateProductRequest và UpdateProductRequest.
- ProductMappingProfile cho AutoMapper.

## Các Lưu Ý Test

### Unit Tests

- Test logic dịch vụ ProductService (CRUD, ABC phân loại).
- Test truy vấn repository ProductRepository (join, filter date).
- Test endpoint ProductsController.
- Test validator.

### Integration Tests

- Test luồng API hoàn chỉnh (upload ảnh, ABC với filter date).
- Test thao tác cơ sở dữ liệu.
- Test chính sách phân quyền.

## Các Cải Tiến Tương Lai

1. **Import/Export Sản Phẩm**: Hỗ trợ bulk import từ Excel/CSV.
2. **Tìm Kiếm Nâng Cao**: Filter theo giá, danh mục, nhà cung cấp, ABC class.
3. **Quản Lý Hình Ảnh**: Resize/thumbnail ảnh, lazy loading.
4. **Audit Trail**: Theo dõi thay đổi sản phẩm.
5. **Tích Hợp Với Tồn Kho**: Tự động cập nhật inventory khi thêm sản phẩm.
6. **Báo Cáo ABC**: Export báo cáo PDF/Excel với chart phân loại.

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

Endpoint Product yêu cầu xác thực JWT. Đảm bảo cấu hình JWT:

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

### Tạo Sản Phẩm Mới Với Hình Ảnh

```bash
POST /api/products
Authorization: Bearer <jwt-token>
Content-Type: multipart/form-data

# Form fields:
# categoryId: 1
# supplierId: 1
# productName: Coca Cola
# barcode: 123456789
# price: 1.99
# unit: can
# Image: (chọn file coca-cola.jpg)
```

### Lấy Danh Sách Sản Phẩm Với Phân Trang

```bash
GET /api/products?pageNumber=1&pageSize=10
Authorization: Bearer <jwt-token>
```

### Cập Nhật Sản Phẩm Với Hình Ảnh Mới

```bash
PUT /api/products/1
Authorization: Bearer <jwt-token>
Content-Type: multipart/form-data

# Form fields:
# productName: Pepsi
# price: 2.00
# Image: (chọn file pepsi.jpg, sẽ xóa coca-cola.jpg cũ)
```

### Lấy Sản Phẩm Theo ID

```bash
GET /api/products/1
Authorization: Bearer <jwt-token>
```

### Phân Tích ABC Với Filter Thời Gian

```bash
GET /api/products/abc-analysis?fromDate=2025-10-01&toDate=2025-10-31&pageNumber=1&pageSize=5
Authorization: Bearer <jwt-token>
```

### Xóa Sản Phẩm

```bash
DELETE /api/products/1
Authorization: Bearer <jwt-token>
```

## Kết Luận

Triển khai API Quản Lý Sản Phẩm tuân thủ nguyên tắc Clean Architecture với sự phân tách trách nhiệm rõ ràng, validation toàn diện, phân quyền dựa trên role, và xử lý lỗi mạnh mẽ. Phân trang được xử lý ở mức controller để tối ưu hiệu suất và linh hoạt. API hỗ trợ upload hình ảnh an toàn, liên kết với danh mục/nhà cung cấp, kiểm tra mã vạch duy nhất, và phân tích ABC với filter thời gian, phù hợp cho hệ thống quản lý bán lẻ.