Chắc chắn rồi, đây là nội dung cập nhật cho tệp `doc/Inventory_API_Implementation.md`, bao gồm các chức năng Nhập hàng (Purchases) và Điều chỉnh (Adjustments) mới, cùng các trường hợp kiểm thử (test cases) mà chúng ta đã thảo luận.

-----

# Triển Khai API Quản Lý Tồn Kho (Inventory API) - Cập Nhật

## 1\. Tổng Quan

Tài liệu này mô tả triển khai API cho toàn bộ **vòng đời quản lý tồn kho** (Inventory Lifecycle) cho hệ thống Store Management.

Thay vì chỉ quản lý số lượng tồn kho qua một API duy nhất, hệ thống giờ đây xử lý tồn kho (`inventory`) như một "sổ cái" trung tâm, được cập nhật tự động bởi **ba luồng nghiệp vụ** riêng biệt:

1.  **API Bán Hàng (Orders API)**: **TRỪ** tồn kho khi đơn hàng được thanh toán (Xuất kho).
2.  **API Nhập Hàng (Purchases API) [MỚI]**: **CỘNG** tồn kho khi đơn nhập hàng được xác nhận (Nhập kho).
3.  **API Điều Chỉnh (Inventory Adjustments API) [MỚI]**: **CỘNG/TRỪ** tồn kho cho các lý do khác (hàng hỏng, mất, kiểm kho, trả hàng).

Tài liệu này sẽ mô tả cả ba thành phần này và cách chúng tương tác với bảng `inventory`.

## 2\. Kiến Trúc

### Các Lớp (Layers)

  - **Lớp Domain**: Entities (`Inventory`, `Purchase`, `PurchaseItem`, `InventoryAdjustment`) và các Interfaces (`IInventoryRepository`, `IPurchaseRepository`, `IInventoryAdjustmentRepository`).
  - **Lớp Application**: DTOs, Services (logic nghiệp vụ), Validators, và AutoMapper Profiles.
  - **Lớp Infrastructure**: Triển khai Repositories (EF Core) và cấu hình DbContext.
  - **Lớp API**: Controllers (`InventoryController`, `PurchasesController`, `InventoryAdjustmentsController`).

## 3\. Chi Tiết Triển Khai (Entities Mới)

### 3.1. Lớp Domain

#### Entity Purchase

```csharp
// StoreManagement.Domain/Entities/Purchase.cs
public class Purchase
{
    public int PurchaseId { get; set; }
    public int? SupplierId { get; set; }
    public int? UserId { get; set; }
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Pending;
    public decimal TotalAmount { get; set; }
    // ...
    public virtual ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
}
```

#### Entity PurchaseItem

```csharp
// StoreManagement.Domain/Entities/PurchaseItem.cs
public class PurchaseItem
{
    public int PurchaseItemId { get; set; }
    public int PurchaseId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal PurchasePrice { get; set; } // Giá nhập
    public decimal Subtotal { get; set; } // Tự tính
    // ...
}
```

#### Entity InventoryAdjustment

```csharp
// StoreManagement.Domain/Entities/InventoryAdjustment.cs
public class InventoryAdjustment
{
    public int AdjustmentId { get; set; }
    public int ProductId { get; set; }
    public int? UserId { get; set; }
    public int Quantity { get; set; } // Số âm (trừ) hoặc dương (cộng)
    public string Reason { get; set; } = string.Empty;
    // ...
}
```

### 3.2. Lớp Application (Logic Nghiệp Vụ)

#### Dịch Vụ `PurchaseService`

  - **`CreatePurchaseAsync`**: Tạo đơn nhập hàng (status `Pending`), tính `TotalAmount` dựa trên `purchasePrice` và `quantity` của các `items`.
  - **`ConfirmPurchaseAsync`**:
    1.  Kiểm tra đơn hàng có tồn tại và đang `Pending` không.
    2.  Lặp qua tất cả `purchase_items`.
    3.  Với mỗi item, tìm `inventory` tương ứng.
    4.  Nếu `inventory` không tồn tại, tạo mới.
    5.  **CỘNG** `item.Quantity` vào `inventory.Quantity`.
    6.  Chuyển `purchase.Status` thành `Confirmed`.
    7.  Lưu tất cả thay đổi (cập nhật kho và đơn hàng) trong một giao dịch (qua `SaveChangesAsync()`).
  - **`CancelPurchaseAsync`**: Chuyển status sang `Canceled` (chỉ khi đang `Pending`). Không cần hoàn kho vì hàng chưa bao giờ được cộng.

#### Dịch Vụ `InventoryAdjustmentService`

  - **`CreateAdjustmentAsync`**:
    1.  Tìm `inventory` của sản phẩm.
    2.  Nếu `inventory` không tồn tại và `quantity` âm, báo lỗi.
    3.  **CỘNG/TRỪ** `request.Quantity` vào `inventory.Quantity`.
    4.  Nếu kết quả tồn kho \< 0, báo lỗi "Insufficient stock".
    5.  Tạo một bản ghi `InventoryAdjustment` mới để lưu lại lịch sử.
    6.  Lưu cả hai thay đổi (cập nhật kho và tạo log) trong một giao dịch.

### 3.3. Lớp API (Endpoints)

#### `InventoryController` (Quản lý chung)

##### GET /api/inventory

  - **Mô Tả**: Lấy danh sách tồn kho (số lượng hiện tại) với join sản phẩm và phân trang.
  - **Phân Quyền**: Staff, Admin.
  - **Tham Số**: `pageNumber`, `pageSize`, `productId` (lọc theo sản phẩm).
  - **Phản Hồi**: `PagedResult<InventoryResponse>`

##### GET /api/inventory/{id}

  - **Mô Tả**: Lấy tồn kho theo ID của entry tồn kho.
  - **Phân Quyền**: Staff, Admin.
  - **Phản Hồi**: `InventoryResponse`

##### GET /api/inventory/low-stock

  - **Mô Tả**: Lấy danh sách tồn kho thấp (low stock) với ngưỡng `threshold`.
  - **Phân Quyền**: Staff, Admin.
  - **Phản Hồi**: `IEnumerable<LowStockResponse>`

#### `PurchasesController` (Nhập kho) [MỚI]

##### POST /api/purchases

  - **Mô Tả**: Tạo một đơn nhập hàng mới (status `Pending`).
  - **Phân Quyền**: Admin only.
  - **Body**: `CreatePurchaseRequest` (gồm `supplierId`, `notes`, `List<items>`).
  - **Phản Hồi**: `PurchaseResponse` (201 Created)

##### GET /api/purchases

  - **Mô Tả**: Lấy danh sách tất cả đơn nhập hàng (phân trang, lọc, tìm kiếm).
  - **Phân Quyền**: Staff, Admin.
  - **Tham Số**: `pageNumber`, `pageSize`, `status`, `supplierId`, `searchTerm` (tìm theo `notes`, `supplier.Name`, `user.FullName`).
  - **Phản Hồi**: `PagedResult<PurchaseResponse>`

##### GET /api/purchases/{id}

  - **Mô Tả**: Lấy chi tiết một đơn nhập hàng (gồm cả `purchaseItems`).
  - **Phân Quyền**: Staff, Admin.
  - **Phản Hồi**: `PurchaseResponse`

##### POST /api/purchases/{id}/confirm

  - **Mô Tả**: Xác nhận đơn nhập hàng. **Kích hoạt logic CỘNG hàng vào `inventory`**.
  - **Phân Quyền**: Admin only.
  - **Phản Hồi**: `PurchaseResponse` (với status `Confirmed`)

##### POST /api/purchases/{id}/cancel

  - **Mô Tả**: Hủy đơn nhập hàng (chỉ khi đang `Pending`).
  - **Phân Quyền**: Admin only.
  - **Phản Hồi**: `PurchaseResponse` (với status `Canceled`)

#### `InventoryAdjustmentsController` (Điều chỉnh kho) [MỚI]

##### POST /api/inventory/adjustments

  - **Mô Tả**: Tạo một phiếu điều chỉnh kho mới. **Kích hoạt logic CỘNG/TRỪ hàng vào `inventory` ngay lập tức**.
  - **Phân Quyền**: Admin only.
  - **Body**: `CreateAdjustmentRequest` (gồm `productId`, `quantity` (âm/dương), `reason`).
  - **Phản Hồi**: `AdjustmentResponse`

##### GET /api/inventory/adjustments

  - **Mô Tả**: Xem lịch sử các lần điều chỉnh kho (phân trang, lọc).
  - **Phân Quyền**: Staff, Admin.
  - **Tham Số**: `pageNumber`, `pageSize`, `productId`.
  - **Phản Hồi**: `PagedResult<AdjustmentResponse>`

## 4\. Các Tính Năng Chính

### 1\. Phân Trang (Pagination)

  - Xử lý phân trang ở mức controller cho `GET /api/inventory`, `GET /api/purchases`, `GET /api/inventory/adjustments`.

### 2\. Join Với Sản Phẩm

  - `InventoryResponse` và `PurchaseItemResponse` tự động join với `Product` để hiển thị thông tin chi tiết.

### 3\. Cảnh Báo Tồn Kho Thấp (Low Stock Alert)

  - Endpoint `GET /api/inventory/low-stock` cho phép truy vấn nhanh các sản phẩm sắp hết hàng.

### 4\. Tự động hóa Tồn Kho [MỚI]

  - Tồn kho được cập nhật tự động khi:
      - `OrdersController` -\> `CheckoutAsync` (Trừ kho)
      - `PurchasesController` -\> `ConfirmPurchaseAsync` (Cộng kho)
      - `InventoryAdjustmentsController` -\> `CreateAdjustmentAsync` (Cộng/Trừ kho)

### 5\. Truy vết Lịch sử (Audit Trail) [MỚI]

  - Mọi thay đổi tồn kho đều được ghi lại.
  - Bán hàng: Ghi trong `order_items`.
  - Nhập hàng: Ghi trong `purchase_items`.
  - Điều chỉnh: Ghi trong `inventory_adjustments`.

## 5\. Schema Cơ Sở Dữ Liệu

### Bảng Inventory (Hiện có)

```sql
CREATE TABLE inventory (
    inventory_id INT AUTO_INCREMENT PRIMARY KEY,
    product_id INT NOT NULL,
    quantity INT DEFAULT 0,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE CASCADE
);
```

### Bảng Purchases (Mới)

```sql
CREATE TABLE purchases (
    purchase_id INT AUTO_INCREMENT PRIMARY KEY,
    supplier_id INT NULL,
    user_id INT NULL,
    status ENUM('pending', 'confirmed', 'canceled') NOT NULL DEFAULT 'pending',
    total_amount DECIMAL(10,2) NOT NULL DEFAULT 0,
    notes TEXT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (supplier_id) REFERENCES suppliers(supplier_id) ON DELETE SET NULL,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE SET NULL
);
```

### Bảng Purchase\_Items (Mới)

```sql
CREATE TABLE purchase_items (
    purchase_item_id INT AUTO_INCREMENT PRIMARY KEY,
    purchase_id INT NOT NULL,
    product_id INT NOT NULL,
    quantity INT NOT NULL,
    purchase_price DECIMAL(10,2) NOT NULL,
    subtotal DECIMAL(10,2) GENERATED ALWAYS AS (quantity * purchase_price) STORED,
    FOREIGN KEY (purchase_id) REFERENCES purchases(purchase_id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE RESTRICT
);
```

### Bảng Inventory\_Adjustments (Mới)

```sql
CREATE TABLE inventory_adjustments (
    adjustment_id INT AUTO_INCREMENT PRIMARY KEY,
    product_id INT NOT NULL,
    user_id INT NULL,
    quantity INT NOT NULL COMMENT 'Số lượng điều chỉnh (âm hoặc dương)',
    reason VARCHAR(255) NOT NULL,
    notes TEXT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE SET NULL
);
```

## 6\. Quy Tắc Validation (Mới)

### CreatePurchaseRequest

  - `SupplierId`: Phải lớn hơn 0 (nếu có).
  - `Items`: Không được rỗng.
  - `Items.ProductId`: Phải lớn hơn 0.
  - `Items.Quantity`: Phải lớn hơn 0.
  - `Items.PurchasePrice`: Phải \>= 0.

### CreateAdjustmentRequest

  - `ProductId`: Phải lớn hơn 0.
  - `Quantity`: Không được bằng 0 (phải là số âm hoặc dương).
  - `Reason`: Không được rỗng.

## 7\. Các Trường Hợp Kiểm Thử (Test Cases)

Dưới đây là các kịch bản kiểm thử chính cho các luồng nghiệp vụ mới, ở định dạng tệp `.http`.

### Biến Môi Trường (Setup)

```http
@baseUrl = http://localhost:5000
@token = <JWT_TOKEN_CỦA_BẠN>
@purchaseId = 1
@purchaseId_to_cancel = 2
```

### Luồng 1: Nhập Hàng (Purchases)

```http
### [TEST 1] Lấy danh sách đơn nhập hàng (GET ALL)
GET {{baseUrl}}/api/purchases?pageNumber=1&pageSize=10
Authorization: Bearer {{token}}

### [TEST 2] Lấy danh sách đơn nhập hàng (Lọc theo status)
GET {{baseUrl}}/api/purchases?status=Pending
Authorization: Bearer {{token}}

### [TEST 3] Tạo 1 đơn nhập hàng mới (status: Pending)
# @name createPurchase
POST {{baseUrl}}/api/purchases
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "supplierId": 1,
  "notes": "Đơn nhập hàng test",
  "items": [
    {
      "productId": 1,
      "quantity": 10,
      "purchasePrice": 250000
    },
    {
      "productId": 2,
      "quantity": 20,
      "purchasePrice": 90000
    }
  ]
}

### [TEST 4] Lấy chi tiết đơn nhập hàng vừa tạo (Dùng biến @purchaseId)
GET {{baseUrl}}/api/purchases/{{purchaseId}}
Authorization: Bearer {{token}}

### [TEST 5] Xác nhận đơn nhập hàng (Confirm)
# !!! HÀNH ĐỘNG NÀY SẼ CỘNG TỒN KHO !!!
POST {{baseUrl}}/api/purchases/{{purchaseId}}/confirm
Authorization: Bearer {{token}}

### [TEST 6] Kiểm tra lại tồn kho sau khi Confirm (VD: productId=1)
GET {{baseUrl}}/api/inventory?productId=1
Authorization: Bearer {{token}}

### [TEST 7] Hủy một đơn nhập hàng (Phải là đơn 'Pending')
POST {{baseUrl}}/api/purchases/{{purchaseId_to_cancel}}/cancel
Authorization: Bearer {{token}}
```

### Luồng 2: Điều Chỉnh Tồn Kho (Adjustments)

```http
### [TEST 8] Tạo 1 phiếu điều chỉnh (TRỪ KHO - Hàng hỏng)
# !!! HÀNH ĐỘNG NÀY SẼ TRỪ TỒN KHO !!!
POST {{baseUrl}}/api/inventory/adjustments
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "productId": 1,
  "quantity": -2,
  "reason": "Hàng hỏng",
  "notes": "Test trừ kho do hỏng"
}

### [TEST 9] Tạo 1 phiếu điều chỉnh (CỘNG KHO - Khách trả hàng)
# !!! HÀNH ĐỘNG NÀY SẼ CỘNG TỒN KHO !!!
POST {{baseUrl}}/api/inventory/adjustments
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "productId": 2,
  "quantity": 1,
  "reason": "Khách trả hàng",
  "notes": "Test cộng kho do khách trả"
}

### [TEST 10] Lỗi - Tạo phiếu điều chỉnh với Quantity = 0
POST {{baseUrl}}/api/inventory/adjustments
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "productId": 3,
  "quantity": 0,
  "reason": "Test lỗi"
}

### [TEST 11] Lỗi - Tạo phiếu điều chỉnh TRỪ kho quá số lượng tồn
POST {{baseUrl}}/api/inventory/adjustments
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "productId": 1,
  "quantity": -999999,
  "reason": "Test lỗi"
}

### [TEST 12] Xem lịch sử điều chỉnh kho (Tất cả)
GET {{baseUrl}}/api/inventory/adjustments
Authorization: Bearer {{token}}

### [TEST 13] Xem lịch sử điều chỉnh kho (Lọc theo productId=1)
GET {{baseUrl}}/api/inventory/adjustments?productId=1
Authorization: Bearer {{token}}
```

## 8\. Các Cải Tiến Tương Lai

1.  **Tính Giá Vốn Hàng Bán (COGS)**: Dùng `purchase_price` từ `purchase_items` để tính COGS khi bán hàng (`orders`).
2.  **Xử lý Hàng Trả (Returns)**: Tạo module `Returns` riêng, khi xác nhận trả hàng sẽ tự động gọi `InventoryAdjustmentService` để cộng kho.
3.  **Báo Cáo Tồn Kho**: Báo cáo chi tiết lịch sử nhập-xuất-tồn cho từng sản phẩm.
4.  **Cảnh Báo Email**: Gửi thông báo low stock qua email/SMS.
5.  **Audit Trail**: Theo dõi lịch sử thay đổi `quantity` chi tiết hơn.

## 9\. Kết Luận

Với việc bổ sung các module `Purchases` và `InventoryAdjustments`, hệ thống đã hoàn thiện vòng đời quản lý tồn kho. Bảng `inventory` giờ đây là một "sổ cái" đáng tin cậy, phản ánh chính xác số lượng tồn kho thực tế, và mọi thay đổi đều được ghi lại rõ ràng, tuân thủ đúng nguyên tắc Clean Architecture.