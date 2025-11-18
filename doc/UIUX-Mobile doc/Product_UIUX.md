## 🎨 Màn hình 1: Danh sách sản phẩm (Product List)

Đây là màn hình chính nơi khách hàng duyệt xem tất cả sản phẩm.

### 1\. Mô tả Giao diện (UI)

  * **[Thanh tìm kiếm (Search Bar)]**: Nằm ở trên cùng, cho phép người dùng gõ văn bản tìm kiếm.
  * **[Nút Lọc/Sắp xếp (Filter/Sort Button)]**: Một icon (ví dụ: hình cái phễu) bên cạnh thanh tìm kiếm, mở ra một modal/popup để lọc và sắp xếp.
  * **[Lưới sản phẩm (Product Grid)]**: Một danh sách cuộn (ví dụ: `GridView` 2 cột) hiển thị các sản phẩm.
  * **[Thẻ sản phẩm (Product Card)]**: Mỗi ô trong lưới hiển thị:
      * Hình ảnh sản phẩm (`ImagePath`).
      * Tên sản phẩm (`ProductName`).
      * Giá bán (`Price`).
  * **[Chỉ báo tải (Loading Indicator)]**: Hiển thị khi đang gọi API hoặc khi cuộn xuống cuối để tải trang tiếp theo.

### 2\. Luồng Tương tác (UX) & Chi tiết API

#### a. Tải Màn hình Lần đầu (Initial Load)

  * **UX:** Người dùng mở màn hình sản phẩm.
  * **Hành động:** Ứng dụng tự động gọi API để lấy trang sản phẩm đầu tiên.
  * **API:** `GET /api/products`
  * **Quyền truy cập:** `[AllowAnonymous]` (Công khai)
  * **Tham số (Params):**
      * `pageNumber=1`
      * `pageSize=20` (hoặc một con số hợp lý cho di động, tối đa 100)
      * `status=Active` (Rất quan trọng: App di động của khách hàng chỉ nên hiển thị các sản phẩm đang "Active")

#### b. Phân trang (Pagination) - "Cuộn vô tận"

  * **UX:** Người dùng cuộn xuống cuối danh sách.
  * **Hành động:** Ứng dụng phát hiện sự kiện cuộn, kiểm tra nếu `pageNumber < totalPages` (từ JSON trả về), sau đó gọi API để lấy trang tiếp theo (`pageNumber + 1`).
  * **API:** `GET /api/products`
  * **Tham số (Params):**
      * `pageNumber=2` (cho lần tải tiếp theo)
      * `pageSize=20`
      * `status=Active`
      * (Gửi kèm `searchTerm` hoặc `sortBy` nếu đang được áp dụng)

#### c. Tìm kiếm (Search)

  * **UX:** Người dùng gõ "Coca" vào thanh tìm kiếm và nhấn Enter (hoặc sau 500ms).
  * **Hành động:** Ứng dụng gọi lại API với `pageNumber=1` và thêm tham số `searchTerm`.
  * **API:** `GET /api/products`
  * **Tham số (Params):**
      * `searchTerm=Coca`
      * `pageNumber=1`
      * `pageSize=20`
      * `status=Active`
  * **Logic Backend:** `ProductService` sẽ tìm kiếm `searchTerm` trong `ProductName` và `Barcode`.

#### d. Sắp xếp (Sorting)

  * **UX:** Người dùng nhấn nút Lọc/Sắp xếp, chọn "Giá (Cao-Thấp)".
  * **Hành động:** Ứng dụng gọi lại API với `pageNumber=1` và thêm tham số sắp xếp.
  * **API:** `GET /api/products`
  * **Tham số (Params):**
      * `sortBy=price`
      * `sortDesc=true`
      * `pageNumber=1`
      * `pageSize=20`
      * `status=Active`
  * **Các giá trị `sortBy` được hỗ trợ:** "name" (hoặc "productname"), "price", "createdat".

#### e. Lọc (Filtering) - (Có Hạn chế)

  * **UX:** Người dùng nhấn nút Lọc/Sắp xếp, chọn danh mục "Đồ uống".
  * **Hành động:**
    1.  **(API Phụ)** Ứng dụng gọi `GET /api/categories` (đã được set `[AllowAnonymous]`) để lấy danh sách `(categoryId, categoryName)`.
    2.  **(Hạn chế)** API `GET /api/products` hiện **không** hỗ trợ tham số `categoryId`.
    3.  **Cách xử lý (Client-side):** Ứng dụng của bạn sẽ phải tải tất cả sản phẩm (đã lọc theo `searchTerm` nếu có) và sau đó tự lọc lại danh sách trên app di động dựa trên `product.categoryId`. Đây là một điểm có thể cải thiện ở backend trong tương lai.

#### f. Cấu trúc JSON trả về (Cho `GET /api/products`)

[cite\_start]Dữ liệu trả về sẽ được bọc trong `ApiResponse` và `PagedResult`[cite: 20].

```json
{
    "success": true,
    "message": "Products retrieved successfully",
    "data": {
        "items": [ // Đây là mảng các sản phẩm
            {
                "productId": 1,
                "categoryId": 2,
                "supplierId": 1,
                "productName": "Coca Cola lon",
                "barcode": "8900000000001",
                "price": 314838.00,
                "unit": "hộp",
                "createdAt": "2024-10-18T00:00:00Z",
                "imagePath": "/images/products/product_1.jpg",
                "status": "Active"
            }
            // ... (nhiều sản phẩm khác)
        ],
        "totalCount": 50,
        "pageNumber": 1,
        "pageSize": 20,
        "totalPages": 3,
        "hasPreviousPage": false,
        "hasNextPage": true
    },
    "error": null,
    // ...
}
```

  * [cite\_start]**(Nguồn: `ApiResponse.cs`, `PagedResult.cs`, `ProductResponse.cs`)** [cite: 20]

-----

## 🎨 Màn hình 2: Chi tiết sản phẩm (Product Detail)

Màn hình này xuất hiện khi người dùng nhấn vào một "Product Card" từ Màn hình 1.

### 1\. Mô tả Giao diện (UI)

  * **[Hình ảnh lớn (Large Image)]**: Hình ảnh của sản phẩm (`imagePath`).
  * **[Thông tin chính]**:
      * Tên sản phẩm (`productName`).
      * Giá bán (`price`) - hiển thị nổi bật.
  * **[Thông tin chi tiết]**:
      * Mã vạch (`barcode`).
      * Đơn vị tính (`unit`).
  * **[Bộ chọn số lượng (Quantity Stepper)]**: Một nút `(-)` để giảm, một ô hiển thị số (mặc định là 1), và một nút `(+)` để tăng.
  * **[Nút "Thêm vào giỏ hàng" (Add to Cart)]**: Nút bấm lớn, rõ ràng ở cuối màn hình.

### 2\. Luồng Tương tác (UX) & Chi tiết API

#### a. Tải Màn hình (Loading the Screen)

  * **UX:** Người dùng nhấn vào sản phẩm "Coca Cola lon" (có `productId: 1`) từ Màn hình 1.
  * **Hành động:** Ứng dụng điều hướng sang Màn hình 2 và gọi API để lấy chi tiết sản phẩm.
  * **API:** `GET /api/products/{id}`
  * **Quyền truy cập:** `[AllowAnonymous]`
  * **Tham số (Params):** `id=1` (trong URL)

#### b. Cấu trúc JSON trả về (Cho `GET /api/products/{id}`)

[cite\_start]Dữ liệu trả về được bọc trong `ApiResponse`[cite: 20].

```json
{
    "success": true,
    "message": "Product retrieved successfully",
    "data": { // Đây là đối tượng ProductResponse
        "productId": 1,
        "categoryId": 2,
        "supplierId": 1,
        "productName": "Coca Cola lon",
        "barcode": "8900000000001",
        "price": 314838.00,
        "unit": "hộp",
        "createdAt": "2024-10-18T00:00:00Z",
        "imagePath": "/images/products/product_1.jpg",
        "status": "Active"
    },
    "error": null,
    // ...
}
```

  * [cite\_start]**(Nguồn: `ApiResponse.cs`, `ProductResponse.cs`)** [cite: 20]

#### c. Thêm vào giỏ hàng

  * **UX:** Người dùng chọn số lượng là 2 và nhấn nút "Thêm vào giỏ hàng".
  * **Hành động:** Ứng dụng **không gọi API nào cả**. [cite\_start]Thay vào đó, nó sẽ lấy thông tin sản phẩm (từ `data` ở trên) và số lượng (từ UI) rồi lưu vào CSDL **SQLite** cục bộ trên thiết bị di động, đúng theo yêu cầu của đề tài[cite: 4, 9, 10].

-----

## 🚫 Tổng hợp Lỗi và Cách Xử lý

Đây là các kịch bản lỗi liên quan đến API Product mà ứng dụng di động của bạn cần xử lý:

### 1\. 404 Not Found (Không tìm thấy)

  * **Kịch bản:** Người dùng cố gắng truy cập chi tiết sản phẩm đã bị xóa hoặc không tồn tại (ví dụ: `GET /api/products/9999`).
  * [cite\_start]**JSON trả về:** [cite: 20]
    ```json
    {
        "success": false,
        "message": "Operation failed",
        "data": null,
        "error": "Product not found"
        // ...
    }
    ```
  * **Xử lý (UX):** Hiển thị thông báo "Không tìm thấy sản phẩm" và cho phép người dùng quay lại Màn hình 1.

### 2\. 401 Unauthorized (Chưa xác thực)

  * **Kịch bản:** Ứng dụng di động của bạn cố gắng gọi một API yêu cầu quyền Admin (ví dụ: `POST /api/products` để tạo sản phẩm).
  * **JSON trả về:** (Thường không có body, chỉ có Status Code 401 do Middleware)
  * **Xử lý (UX):** Đây là lỗi logic của lập trình viên. Ứng dụng khách hàng không bao giờ được phép gọi API này. Nếu xảy ra, chỉ cần log lỗi.

### 3\. 500 Internal Server Error (Lỗi máy chủ)

  * **Kịch bản:** Máy chủ backend gặp lỗi (ví dụ: mất kết nối CSDL).
  * **JSON trả về:** (Được xử lý bởi `GlobalExceptionMiddleware`)
    ```json
    {
        "success": false,
        "message": "Internal server error",
        "data": null,
        "error": "An unexpected error occurred"
        // ...
    }
    ```
  * **Xử lý (UX):** Hiển thị thông báo chung chung ("Đã có lỗi xảy ra. Vui lòng thử lại sau.") và có nút "Thử lại" (Retry) để gọi lại API.