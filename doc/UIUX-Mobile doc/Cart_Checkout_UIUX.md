## 🎨 Màn hình 3: Giỏ hàng (Shopping Cart)

Màn hình này hiển thị các sản phẩm mà người dùng đã thêm vào giỏ hàng từ Màn hình 1 và 2.

### 1\. Mô tả Giao diện (UI)

  * **[Danh sách sản phẩm (Cart Item List)]**: Một danh sách cuộn (`ListView`) hiển thị các mặt hàng trong giỏ.
  * **[Thẻ mặt hàng (Cart Item Card)]**: Mỗi hàng trong danh sách hiển thị:
      * Hình ảnh (lấy từ `imagePath` của sản phẩm).
      * Tên sản phẩm.
      * Giá (`price`).
      * **[Bộ chọn số lượng (Quantity Stepper)]**: Nút `(-)` và `(+)` để thay đổi số lượng.
      * **[Nút Xóa (Delete Button)]**: Icon thùng rác để xóa mặt hàng.
  * **[Khu vực Khuyến mãi (Promo Section)]**:
      * Một ô nhập liệu: "Nhập mã khuyến mãi".
      * Nút "Áp dụng".
  * **[Tóm tắt (Summary)]**:
      * Tổng tiền hàng (Tạm tính).
      * Giảm giá (Hiển thị sau khi áp dụng mã).
      * **Tổng cộng (Final Total)**.
  * **[Nút "Tiếp tục thanh toán" (Checkout Button)]**: Nút lớn ở cuối màn hình.

### 2\. Luồng Tương tác (UX) & Chi tiết API

#### a. Tải Màn hình (Initial Load)

  * **UX:** Người dùng nhấn vào icon giỏ hàng.
  * **Hành động:** Ứng dụng **không gọi API**. Thay vào đó, nó truy vấn CSDL **SQLite** cục bộ.
  * [cite\_start]**Truy vấn (SQLite):** `SELECT * FROM CartItem` (đây là bảng bạn sẽ tạo trong app Uno, theo yêu cầu đề tài [cite: 46]).
  * [cite\_start]**Logic:** Tải danh sách, tính toán và hiển thị "Tổng cộng"[cite: 49].

#### b. Thay đổi số lượng

  * **UX:** Người dùng nhấn nút `(+)` hoặc `(-)` trên một mặt hàng.
  * **Hành động:** Ứng dụng cập nhật CSDL **SQLite**.
  * [cite\_start]**Truy vấn (SQLite):** `UPDATE CartItem SET Quantity = [new_quantity] WHERE ProductId = [id]`[cite: 49].
  * **Logic:** Tính toán lại "Tổng cộng" và cập nhật UI.

#### c. Xóa mặt hàng

  * **UX:** Người dùng nhấn nút Xóa (thùng rác).
  * **Hành động:** Ứng dụng xóa mặt hàng khỏi **SQLite**.
  * [cite\_start]**Truy vấn (SQLite):** `DELETE FROM CartItem WHERE ProductId = [id]`[cite: 49].
  * **Logic:** Tính toán lại "Tổng cộng" và cập nhật UI.

#### d. Áp dụng Mã khuyến mãi

  * **UX:** Người dùng nhập "SALE10" và nhấn "Áp dụng".
  * **Hành động:** Ứng dụng gọi API backend để xác thực mã.
  * **API:** `GET /api/promotion/by-code/SALE10`
  * **Quyền truy cập:** `[AllowAnonymous]` (như chúng ta đã cập nhật).
  * **Tham số (Params):** `promoCode=SALE10` (trong URL).
  * **Cấu trúc JSON trả về (Ví dụ):**
    ```json
    {
        "success": true,
        "data": {
            "promoId": 1,
            "promoCode": "SALE10",
            "discountType": "Percent",
            "discountValue": 10,
            "minOrderAmount": 100000,
            // ... (các trường khác)
        }
    }
    ```
  * **Logic (UX):**
    1.  Kiểm tra `success: true`.
    2.  Kiểm tra `data.minOrderAmount` so với "Tạm tính".
    3.  Nếu hợp lệ, lưu `promoId` và `discountValue` vào một biến tạm (state) của ứng dụng.
    4.  Tính toán lại "Tổng cộng" (có trừ khuyến mãi) và hiển thị số tiền được giảm.

#### e. Tiếp tục Thanh toán

  * **UX:** Người dùng nhấn nút "Tiếp tục thanh toán".
  * **Hành động:** Điều hướng sang Màn hình 4.

-----

## 🎨 Màn hình 4: Thanh toán (Checkout)

Màn hình cuối cùng, thu thập thông tin khách hàng và gửi đơn hàng.

### 1\. Mô tả Giao diện (UI)

  * [cite\_start]**[Form Thông tin Khách hàng]**: Gồm các ô nhập liệu[cite: 49]:
      * Họ và Tên (`CustomerName`)
      * Số điện thoại (`CustomerPhone`)
      * Email (`CustomerEmail`)
      * Địa chỉ (`CustomerAddress`)
  * **[Tóm tắt Đơn hàng]**: Hiển thị lại "Tổng cộng" (đã bao gồm khuyến mãi) được truyền từ Màn hình 3.
  * **[Phương thức Thanh toán]**: Các nút chọn (`RadioButton` hoặc `SegmentedControl`):
      * "Tiền mặt" (Giá trị: `Cash`)
      * "Thẻ" (Giá trị: `Card`)
      * "Ví điện tử" (Giá trị: `EWallet`)
      * [cite\_start]*(Các giá trị này phải khớp với Enum `PaymentMethod` của backend [cite: 50])*
  * **[Nút "Đặt hàng" (Place Order)]**: Nút cuối cùng để xác nhận.

### 2\. Luồng Tương tác (UX) & Chi tiết API

#### a. Tải Màn hình

  * **UX:** Người dùng được điều hướng từ Giỏ hàng sang.
  * **Hành động:** Màn hình hiển thị Form và "Tổng cộng" (đã tính toán ở bước trước).

#### b. [cite\_start]Đặt hàng (Logic quan trọng nhất) [cite: 49]

  * **UX:** Người dùng điền đầy đủ thông tin và nhấn "Đặt hàng".

  * **Hành động:** Ứng dụng tập hợp *tất cả* dữ liệu (từ Form, từ SQLite, từ biến tạm) và gọi API `POST /api/orders`.

  * **API:** `POST /api/orders`

  * **Quyền truy cập:** `[AllowAnonymous]`

  * **Tham số (Params):** Gửi một đối tượng JSON duy nhất trong Body.

  * **Cấu trúc JSON gửi đi (Ví dụ):**

    ```json
    {
        "customerId": null,
        "customerName": "Nguyễn Văn A",
        "customerPhone": "0987654321",
        "customerEmail": "a@test.com",
        "customerAddress": "123 Đường ABC, Q1, TPHCM",

        "orderDetails": [ 
            { "productId": 1, "quantity": 2 },
            { "productId": 2, "quantity": 1 }
        ],

        "paymentMethod": "Cash",
        "amountPaid": 744483, 
        "promoId": null
    }
    ```

      * **(Nguồn DTO: `CreateOrderRequest.cs` chúng ta đã cập nhật)**
      * `customerName`, `customerPhone`...: Lấy từ Form.
      * `orderDetails`: Lấy từ **SQLite** (`SELECT * FROM CartItem`).
      * `paymentMethod`: Lấy từ lựa chọn của người dùng.
      * `amountPaid`: Lấy từ "Tổng cộng" đã tính.
      * `promoId`: Lấy từ biến tạm (nếu đã áp dụng mã).

### 3\. Xử lý Phản hồi (Response Handling)

#### a. [cite\_start]Kịch bản Thành công (Success) [cite: 49]

  * **JSON trả về (201 Created):**
    ```json
    {
        "success": true,
        "message": "Order created successfully",
        "data": {
            "orderId": 123,
            "status": "Paid",
            // ... (thông tin đơn hàng đầy đủ)
        }
    }
    ```
  * **Xử lý (UX):**
    1.  Hiển thị thông báo "Đặt hàng thành công\!".
    2.  **Quan trọng:** Xóa giỏ hàng cục bộ: `DELETE FROM CartItem`.
    3.  Điều hướng người dùng về Màn hình 1.

#### b. [cite\_start]Kịch bản Lỗi (Error) [cite: 49]

  * **Kịch bản 1: Hết hàng (400 Bad Request)**
    ```json
    {
        "success": false,
        "message": "Order creation failed",
        "error": "Sản phẩm (ID: 1) không đủ tồn kho."
    }
    ```
  * **Kịch bản 2: Sai số tiền (400 Bad Request)**
    ```json
    {
        "success": false,
        "message": "Order creation failed",
        "error": "Số tiền thanh toán (740000) không khớp với tổng đơn hàng (744483)."
    }
    ```
  * **Xử lý (UX):**
    1.  Hiển thị thông báo lỗi (`error`) cho người dùng.
    2.  **Quan trọng:** **Không** xóa giỏ hàng SQLite.
    3.  Giữ người dùng ở Màn hình 4 (hoặc tự động điều hướng về Màn hình 3) để họ sửa lại (ví dụ: giảm số lượng sản phẩm bị hết hàng).