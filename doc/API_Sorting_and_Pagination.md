# API Sorting and Pagination

This document describes the unified sorting and pagination behavior across all list endpoints.

## Query parameters

- pageNumber: integer >= 1 (default 1)
- pageSize: integer (typical default 10; max enforced by controllers if any)
- searchTerm: optional string (entity-specific)
- sortBy: optional string field name (whitelisted per entity)
- sortDesc: optional boolean (default false); false=ascending, true=descending

## Defaults and supported sortBy per entity

### Users

- Default: UserId ASC, tie-breaker UserId
- sortBy: id | username | fullname | role
  - Example: /api/users?pageNumber=1&pageSize=10&sortBy=id&sortDesc=true

### Products

- Default: ProductId ASC, tie-breaker ProductId
- sortBy: id | name | productName | price | createdAt
  - Example: /api/products?pageNumber=1&pageSize=10&sortBy=price&sortDesc=true

### Categories

- Default: CategoryId ASC, tie-breaker CategoryId
- sortBy: id | name | categoryName
  - Example: /api/categories?pageNumber=1&pageSize=10&sortBy=id

### Suppliers

- Default: SupplierId ASC, tie-breaker SupplierId
- sortBy: id | name | email
  - Example: /api/suppliers?pageNumber=1&pageSize=10&sortBy=name&sortDesc=true

### Inventory

- Default: InventoryId ASC, tie-breaker InventoryId
- sortBy: id | productId | quantity
  - Example: /api/inventory?pageNumber=1&pageSize=10&sortBy=quantity&sortDesc=true

### Customers

- Default: CustomerId ASC, tie-breaker CustomerId
- sortBy: id | name | email | phone
  - Example: /api/customer?pageNumber=1&pageSize=10&sortBy=id

### Promotions

- Default: PromoId ASC, tie-breaker PromoId
- sortBy: id | promoId | code | promoCode | startDate | endDate | status
  - Example: /api/promotion?pageNumber=1&pageSize=10&sortBy=startDate&sortDesc=true

## Stable pagination

All services apply a secondary tie-breaker on the entity Id to keep pagination stable when records share the same primary sorted value or when data changes between page requests.

## Notes

- Sorting and filtering are executed in the database via repository `GetPagedAsync`, ensuring performance for large datasets.
- `searchTerm` behavior varies slightly by entity (e.g., Users: Username/FullName; Products: ProductName/Barcode; Customers: Name/Email/Phone).
