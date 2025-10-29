Chắc chắn rồi\! Dưới đây là tài liệu chi tiết về module Báo cáo mới, bao gồm giải thích các endpoint, cấu trúc dữ liệu và các ví dụ test cụ thể để đội ngũ Frontend có thể dễ dàng tích hợp và sử dụng.

-----

# **Tài liệu API Module Báo cáo (Reports API)**

## **Tổng quan**

Module Báo cáo cung cấp các endpoint để truy xuất dữ liệu phân tích và tổng hợp về tình hình kinh doanh, giúp đưa ra các quyết định quan trọng. Các báo cáo này được tối ưu để thực hiện các phép tính phức tạp ở tầng cơ sở dữ liệu, đảm bảo hiệu năng cao.

Tất cả các endpoint báo cáo đều nằm dưới một controller chung là `ReportsController` và yêu cầu quyền truy cập của `Admin` hoặc `Staff`.

**URL gốc:** `/api/reports`

-----

## **1. Báo cáo Tổng quan Doanh thu (Sales Overview)**

Endpoint này cung cấp dữ liệu tổng hợp về doanh thu, số lượng đơn hàng, và giá trị trung bình của đơn hàng trong một khoảng thời gian nhất định, có thể được nhóm theo ngày hoặc tháng.

  * **Endpoint:** `GET /api/reports/sales/overview`
  * **Mô tả:** Lấy dữ liệu tổng hợp doanh thu từ các đơn hàng đã được thanh toán (`status = 'Paid'`).
  * **Quyền truy cập:** `Admin`, `Staff`.

### **Tham số (Query Parameters)**

| Tên tham số | Kiểu dữ liệu | Bắt buộc | Mặc định | Mô tả |
| :--- | :--- | :--- | :--- | :--- |
| `startDate` | `DateTime` | Có | | Ngày bắt đầu của kỳ báo cáo (định dạng: `YYYY-MM-DD`). |
| `endDate` | `DateTime` | Có | | Ngày kết thúc của kỳ báo cáo (định dạng: `YYYY-MM-DD`). |
| `groupBy` | `string` | Không | `"day"` | Đơn vị nhóm dữ liệu. Chấp nhận các giá trị: `"day"`, `"month"`. |

### **Cấu trúc Phản hồi (Response Body)**

Phản hồi là một mảng các đối tượng `SalesSummaryResponse`:

  * **`period` (string):** Chu kỳ thời gian được nhóm (`YYYY-MM-DD` nếu nhóm theo ngày, `YYYY-MM` nếu nhóm theo tháng).
  * **`totalRevenue` (decimal):** Tổng doanh thu thực tế (đã trừ khuyến mãi) trong chu kỳ đó.
  * **`numberOfOrders` (int):** Tổng số đơn hàng đã thanh toán trong chu kỳ.
  * **`averageOrderValue` (decimal):** Giá trị trung bình của mỗi đơn hàng (`totalRevenue / numberOfOrders`).

### **Ví dụ Yêu cầu và Phản hồi**

#### **Yêu cầu:** Lấy báo cáo doanh thu theo ngày từ 01/01/2025 đến 31/12/2025.

```http
GET http://localhost:5000/api/reports/sales/overview?startDate=2025-01-01&endDate=2025-12-31&groupBy=day
Authorization: Bearer <your_jwt_token>
```

#### **Phản hồi Thành công (200 OK):**

```json
{
    "success": true,
    "message": "Sales overview retrieved successfully.",
    "data": [
        {
            "period": "2025-10-15",
            "totalRevenue": 82885994.40,
            "numberOfOrders": 60,
            "averageOrderValue": 1381433.24
        },
        {
            "period": "2025-10-29",
            "totalRevenue": 462539.00,
            "numberOfOrders": 1,
            "averageOrderValue": 462539.00
        }
    ],
    "error": null,
    "errors": null,
    "timestamp": "2025-10-29T05:05:05.8317145Z"
}
```

#### **Phản hồi Lỗi (400 Bad Request):** Khi `startDate` sau `endDate`.

```json
{
    "success": false,
    "message": "Operation failed",
    "data": null,
    "error": "startDate cannot be after endDate.",
    "timestamp": "2025-10-29T05:10:00Z"
}
```

-----

## **2. Báo cáo Hàng tồn kho không bán được (Dead Stock)**

Endpoint này giúp xác định các sản phẩm không có bất kỳ giao dịch mua bán nào trong một khoảng thời gian được chọn, giúp quản lý kho hàng hiệu quả hơn.

  * **Endpoint:** `GET /api/reports/products/dead-stock`
  * **Mô tả:** Lấy danh sách các sản phẩm không được bán trong khoảng thời gian đã chọn.
  * **Quyền truy cập:** `Admin`, `Staff`.

### **Tham số (Query Parameters)**

| Tên tham số | Kiểu dữ liệu | Bắt buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `startDate` | `DateTime` | Có | Ngày bắt đầu của kỳ báo cáo (định dạng: `YYYY-MM-DD`). |
| `endDate` | `DateTime` | Có | Ngày kết thúc của kỳ báo cáo (định dạng: `YYYY-MM-DD`). |

### **Cấu trúc Phản hồi (Response Body)**

Phản hồi là một mảng các đối tượng `DeadStockProductResponse`:

  * **`productId` (int):** ID của sản phẩm.
  * **`productName` (string):** Tên sản phẩm.
  * **`barcode` (string):** Mã vạch của sản phẩm.
  * **`price` (decimal):** Giá bán hiện tại của sản phẩm.
  * **`quantityInStock` (int):** Số lượng tồn kho hiện tại của sản phẩm.

### **Ví dụ Yêu cầu và Phản hồi**

#### **Yêu cầu:** Lấy danh sách sản phẩm không bán được trong cả năm 2025.

```http
GET http://localhost:5000/api/reports/products/dead-stock?startDate=2025-01-01&endDate=2025-12-31
Authorization: Bearer <your_jwt_token>
```

#### **Phản hồi Thành công (200 OK):**

```json
{
    "success": true,
    "message": "Dead stock report retrieved successfully.",
    "data": [
        {
            "productId": 6,
            "productName": "Bánh Oreo",
            "barcode": "8900000000006",
            "price": 209283.00,
            "quantityInStock": 105
        },
        {
            "productId": 13,
            "productName": "Muối i-ốt",
            "barcode": "8900000000013",
            "price": 173302.00,
            "quantityInStock": 46
        }
    ],
    "error": null,
    "errors": null,
    "timestamp": "2025-10-29T04:58:56.9624311Z"
}
```

-----

## **Hướng dẫn Test cho Frontend**

Để giúp đội ngũ Frontend dễ dàng kiểm thử và tích hợp, chúng tôi đã tạo một file `Reports_API_Tests.http`. File này có thể được sử dụng với extension **REST Client** trong Visual Studio Code.

### **Nội dung file `Reports_API_Tests.http`**

```http
# URL gốc của API
@baseUrl = http://localhost:5000

# Dán token nhận được sau khi đăng nhập vào đây
@token = eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

###
# ======================================================
# BÁO CÁO DOANH THU (SALES REPORTS)
# ======================================================

### [TEST 1] Lấy báo cáo doanh thu theo NGÀY
#
GET {{baseUrl}}/api/reports/sales/overview?startDate=2025-01-01&endDate=2025-12-31&groupBy=day
Authorization: Bearer {{token}}
Accept: application/json

### [TEST 2] Lấy báo cáo doanh thu theo THÁNG
#
GET {{baseUrl}}/api/reports/sales/overview?startDate=2025-01-01&endDate=2025-12-31&groupBy=month
Authorization: Bearer {{token}}
Accept: application/json

### [TEST 3] Lỗi: startDate sau endDate
#
GET {{baseUrl}}/api/reports/sales/overview?startDate=2025-12-31&endDate=2025-01-01
Authorization: Bearer {{token}}
Accept: application/json

###
# ======================================================
# BÁO CÁO HÀNG TỒN KHO KHÔNG BÁN ĐƯỢC (DEAD STOCK)
# ======================================================

### [TEST 4] Lấy danh sách sản phẩm không bán được trong năm
#
GET {{baseUrl}}/api/reports/products/dead-stock?startDate=2025-01-01&endDate=2025-12-31
Authorization: Bearer {{token}}
Accept: application/json

```

### **Cách sử dụng:**

1.  **Cài đặt Extension:** Cài đặt "REST Client" trong VS Code.
2.  **Lấy Token:** Chạy API, dùng một endpoint đăng nhập để lấy JWT token.
3.  **Cập nhật Token:** Dán token vào biến `@token` ở đầu file `.http`.
4.  **Gửi Yêu cầu:** Click vào nút "Send Request" xuất hiện phía trên mỗi yêu cầu để thực thi và xem kết quả trực tiếp.