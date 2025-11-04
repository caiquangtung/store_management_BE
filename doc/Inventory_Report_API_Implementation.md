# Tài liệu API Module Báo cáo (Cập nhật)

  * **Controller:** `ReportsController`
  * **URL Gốc:** `/api/reports`
  * **Quyền truy cập:** Yêu cầu `Admin` hoặc `Staff` (Tất cả endpoints)

## 1\. Báo cáo Sổ Kho (Inventory Ledger)

Báo cáo này cung cấp một bản sao kê chi tiết, theo thứ tự thời gian, về mọi thay đổi tồn kho (nhập, bán, điều chỉnh) cho một sản phẩm cụ thể trong một khoảng thời gian.

  * **Endpoint:** `GET /api/reports/inventory/ledger`
  * **Mô tả:** Truy vết lịch sử di chuyển của một sản phẩm để đối chiếu tồn kho.
  * **Quyền truy cập:** `Admin`, `Staff`.

### Tham số (Query Parameters)

| Tên tham số | Kiểu dữ liệu | Bắt buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `productId` | `int` | Có | ID của sản phẩm cần xem sổ kho. |
| `startDate` | `DateTime` | Có | Ngày bắt đầu của kỳ báo cáo (định dạng: `YYYY-MM-DD`). |
| `endDate` | `DateTime` | Có | Ngày kết thúc của kỳ báo cáo (định dạng: `YYYY-MM-DD`). |

### Cấu trúc Phản hồi (Response Body)

Phản hồi là một đối tượng `InventoryLedgerResponse`.

```json
{
    "success": true,
    "message": "Inventory ledger retrieved successfully.",
    "data": {
        "productId": 1,
        "productName": "Coca Cola lon",
        "startDate": "2025-01-01T00:00:00Z",
        "endDate": "2025-10-31T00:00:00Z",
        "startQuantity": 25,
        "endQuantity": 31,
        "movements": [
            {
                "date": "2025-10-15T10:30:00Z",
                "type": "Bán hàng",
                "reference": "Order #1005",
                "change": -2,
                "balance": 23
            },
            {
                "date": "2025-10-16T14:00:00Z",
                "type": "Nhập hàng",
                "reference": "Purchase #1",
                "change": 10,
                "balance": 33
            },
            {
                "date": "2025-10-17T09:15:00Z",
                "type": "Hàng hỏng",
                "reference": "Adj. #1",
                "change": -2,
                "balance": 31
            }
        ]
    }
}
```

  * **`productId`**: ID sản phẩm.
  * **`productName`**: Tên sản phẩm.
  * **`startDate` / `endDate`**: Khoảng thời gian báo cáo.
  * **`startQuantity`**: Số lượng tồn kho tại thời điểm `00:00:00` của `startDate`.
  * **`endQuantity`**: Số lượng tồn kho cuối cùng sau giao dịch cuối cùng trong kỳ.
  * **`movements` (Array)**: Danh sách các giao dịch.
      * **`date`**: Ngày giờ chính xác của giao dịch.
      * **`type`**: Loại giao dịch (`Bán hàng`, `Nhập hàng`, hoặc `reason` từ bảng điều chỉnh như `Hàng hỏng`, `Trả hàng`).
      * **`reference`**: Mã tham chiếu (VD: `Order #1005`, `Purchase #1`).
      * **`change`**: Số lượng thay đổi (âm là giảm, dương là tăng).
      * **`balance`**: Số dư tồn kho *sau khi* giao dịch này được thực hiện.

-----

## 2\. Báo cáo Tổng quan Nhập hàng (Purchase Summary)

Báo cáo này cung cấp dữ liệu tổng hợp về tổng chi phí và số lượng đơn nhập hàng đã được xác nhận (`Confirmed`) trong một khoảng thời gian.

  * **Endpoint:** `GET /api/reports/purchases/summary`
  * **Mô tả:** Thống kê tổng chi phí nhập hàng từ các đơn hàng đã `Confirmed`.
  * **Quyền truy cập:** `Admin`, `Staff`.

### Tham số (Query Parameters)

| Tên tham số | Kiểu dữ liệu | Bắt buộc | Mặc định | Mô tả |
| :--- | :--- | :--- | :--- | :--- |
| `startDate` | `DateTime` | Có | Ngày bắt đầu của kỳ báo cáo (định dạng: `YYYY-MM-DD`). |
| `endDate` | `DateTime` | Có | Ngày kết thúc của kỳ báo cáo (định dạng: `YYYY-MM-DD`). |
| `groupBy` | `string` | Không | `"day"` | Đơn vị nhóm dữ liệu. Chấp nhận: `"day"`, `"month"`. |

### Cấu trúc Phản hồi (Response Body)

Phản hồi là một mảng các đối tượng `PurchaseSummaryResponse`.

```json
{
    "success": true,
    "message": "Purchase summary retrieved successfully.",
    "data": [
        {
            "period": "2025-10",
            "totalSpent": 2150000.00,
            "numberOfPurchases": 1
        },
        {
            "period": "2025-11",
            "totalSpent": 5800000.00,
            "numberOfPurchases": 3
        }
    ]
}
```

  * **`period` (string):** Chu kỳ thời gian (`YYYY-MM-DD` nếu nhóm theo ngày, `YYYY-MM` nếu nhóm theo tháng).
  * **`totalSpent` (decimal):** Tổng tiền đã chi cho các đơn nhập hàng (`total_amount` từ bảng `purchases`).
  * **`numberOfPurchases` (int):** Tổng số đơn nhập hàng đã `Confirmed` trong chu kỳ.

-----

## 3\. Gợi ý Giao diện (UI Suggestions)

Đây là các gợi ý đơn giản cho Frontend để mường tượng cách hiển thị dữ liệu.

### 📈 UI cho Tổng quan Nhập hàng (Purchase Summary)

  * **Bộ lọc (Filters):** Cung cấp 2 ô chọn ngày (Start Date, End Date) và một ô Dropdown/Radio button cho "Group By" (Theo Ngày / Theo Tháng).
  * **Hiển thị:**
    1.  **Biểu đồ cột (Bar Chart):**
          * Trục X là `period` (Thời gian).
          * Trục Y là `totalSpent` (Tổng chi).
          * *Mục đích:* Giúp người dùng thấy ngay ngày nào/tháng nào chi nhiều tiền nhập hàng nhất.
    2.  **Bảng dữ liệu (Data Table):**
          * Hiển thị y hệt dữ liệu trả về từ API.
          * Cột: `Thời gian (Period)`, `Tổng chi (TotalSpent)`, `Số đơn nhập (NumberOfPurchases)`.
          * Nên có dòng "Tổng cộng" ở cuối bảng.

### 🧾 UI cho Sổ Kho (Inventory Ledger)

  * **Bộ lọc (Filters):**
    1.  Một ô **Tìm kiếm Sản phẩm (Autocomplete Search Box)** (bắt buộc) để người dùng chọn `productId`.
    2.  Hai ô chọn ngày (Start Date, End Date) (bắt buộc).
  * **Hiển thị:** Báo cáo này không dùng biểu đồ, mà tập trung vào **hiển thị dạng bảng** chi tiết.
    1.  **Thẻ Thông tin (Summary Cards):** Hiển thị các thông tin tổng quan ở đầu trang:
          * **Sản phẩm:** `Coca Cola lon (ID: 1)`
          * **Kỳ báo cáo:** `01/01/2025 - 31/10/2025`
          * **Tồn đầu kỳ:** `25`
          * **Tồn cuối kỳ:** `31`
    2.  **Bảng Sao kê (Ledger Table):**
          * Cột: `Ngày (Date)`, `Loại (Type)`, `Mã Tham chiếu (Reference)`, `Thay đổi (Change)`, `Tồn kho (Balance)`.
          * *Quan trọng:* Các số `Change` âm (ví dụ: -2) nên được tô **màu đỏ**, và số dương (ví dụ: +10) nên được tô **màu xanh lá** để dễ dàng nhận biết.

-----

## 4\. Test Cases (Tệp .http)

Bạn có thể lưu nội dung này vào tệp `Reports_API_Tests.http` và chạy bằng VS Code REST Client.

```http
# URL gốc của API
@baseUrl = http://localhost:5000

# Dán token nhận được sau khi đăng nhập vào đây
@token = <DÁN_TOKEN_CỦA_BẠN_VÀO_ĐÂY>

###
# ======================================================
# BÁO CÁO TỔNG QUAN NHẬP HÀNG (PURCHASE SUMMARY)
# ======================================================

### [TEST 1] Lấy báo cáo nhập hàng theo NGÀY
GET {{baseUrl}}/api/reports/purchases/summary?startDate=2025-01-01&endDate=2025-12-31&groupBy=day
Authorization: Bearer {{token}}
Accept: application/json

### [TEST 2] Lấy báo cáo nhập hàng theo THÁNG
GET {{baseUrl}}/api/reports/purchases/summary?startDate=2025-01-01&endDate=2025-12-31&groupBy=month
Authorization: Bearer {{token}}
Accept: application/json

### [TEST 3] Lỗi: startDate sau endDate
GET {{baseUrl}}/api/reports/purchases/summary?startDate=2025-12-31&endDate=2025-01-01
Authorization: Bearer {{token}}
Accept: application/json

###
# ======================================================
# BÁO CÁO SỔ KHO (INVENTORY LEDGER)
# ======================================================

### [TEST 4] Lấy Sổ kho cho sản phẩm (ví dụ productId=1)
GET {{baseUrl}}/api/reports/inventory/ledger?productId=1&startDate=2025-01-01&endDate=2025-12-31
Authorization: Bearer {{token}}
Accept: application/json

### [TEST 5] Lấy Sổ kho cho sản phẩm khác (ví dụ productId=2)
GET {{baseUrl}}/api/reports/inventory/ledger?productId=2&startDate=2025-01-01&endDate=2025-12-31
Authorization: Bearer {{token}}
Accept: application/json

### [TEST 6] Lỗi: Không tìm thấy sản phẩm (ví dụ productId=9999)
GET {{baseUrl}}/api/reports/inventory/ledger?productId=9999&startDate=2025-01-01&endDate=2025-12-31
Authorization: Bearer {{token}}
Accept: application/json

### [TEST 7] Lỗi: Thiếu tham số productId
GET {{baseUrl}}/api/reports/inventory/ledger?startDate=2025-01-01&endDate=2025-12-31
Authorization: Bearer {{token}}
Accept: application/json
```