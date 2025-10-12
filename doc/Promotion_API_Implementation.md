# Promotion API Implementation

## Overview

This document describes the implementation of the Promotion API for the Store Management system. The Promotion API provides comprehensive promotion management with complex business logic including validation, discount calculation, usage tracking, and time-based management.

## Architecture

### Layers

- **Domain Layer**: Promotion entity, IPromotionRepository interface, and DiscountType enum
- **Application Layer**: Promotion DTOs, services, validators, and AutoMapper profiles
- **Infrastructure Layer**: PromotionRepository implementation and database configuration
- **API Layer**: PromotionController with REST endpoints

## Implementation Details

### 1. Domain Layer

#### Promotion Entity

```csharp
public class Promotion
{
    public int PromoId { get; set; }
    public string PromoCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal MinOrderAmount { get; set; } = 0;
    public int UsageLimit { get; set; } = 0;
    public int UsedCount { get; set; } = 0;
    public string Status { get; set; } = "active";

    // Navigation properties
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
```

#### DiscountType Enum

```csharp
public enum DiscountType
{
    Percent,  // Value = 0, Percentage discount
    Fixed     // Value = 1, Fixed amount discount
}
```

#### IPromotionRepository Interface

```csharp
public interface IPromotionRepository : IRepository<Promotion>
{
    Task<Promotion?> GetByPromoCodeAsync(string promoCode);
    Task<bool> PromoCodeExistsAsync(string promoCode);
    Task<bool> PromoCodeExistsForOtherPromotionAsync(string promoCode, int promotionId);
    Task<IEnumerable<Promotion>> GetActivePromotionsAsync();
    Task<IEnumerable<Promotion>> GetExpiredPromotionsAsync();
    Task IncrementUsageCountAsync(int promotionId);
    Task DeactivateExpiredPromotionsAsync();
}
```

### 2. Application Layer

#### DTOs

- **CreatePromotionRequest**: For creating new promotions (uses DiscountType enum)
- **UpdatePromotionRequest**: For updating existing promotions (uses DiscountType enum)
- **PromotionResponse**: For returning promotion data with computed properties (uses DiscountType enum)
- **ValidatePromotionRequest**: For promotion validation requests
- **PromotionValidationResponse**: For promotion validation results

#### Validators

- **CreatePromotionRequestValidator**: Comprehensive validation for promotion creation
- **UpdatePromotionRequestValidator**: Validation for promotion updates
- **ValidatePromotionRequestValidator**: Validation for promotion validation requests

#### Services

- **IPromotionService**: Interface defining all promotion operations
- **PromotionService**: Implementation with complex business logic

#### AutoMapper Profile

- **PromotionMappingProfile**: Maps between Promotion entity and DTOs with computed properties

### 3. Infrastructure Layer

#### PromotionRepository

```csharp
public class PromotionRepository : BaseRepository<Promotion>, IPromotionRepository
{
    // Specialized methods for promotion management
    // Active promotion queries, usage tracking, expiration management
}
```

#### Database Configuration

Promotion entity is configured in StoreDbContext with proper column mappings and constraints.

### 4. API Layer

#### PromotionController Endpoints

##### GET /api/promotion

- **Description**: Get all promotions with pagination and search
- **Authorization**: Staff, Admin
- **Parameters**:
  - `pageNumber` (optional, default: 1)
  - `pageSize` (optional, default: 10, max: 100)
  - `searchTerm` (optional): Search by promo code or description
- **Response**: PagedResult<PromotionResponse>

##### GET /api/promotion/{id}

- **Description**: Get promotion by ID
- **Authorization**: Staff, Admin
- **Parameters**: `id` (promotion ID)
- **Response**: PromotionResponse

##### GET /api/promotion/by-code/{promoCode}

- **Description**: Get promotion by promo code
- **Authorization**: Staff, Admin
- **Parameters**: `promoCode` (promotion code)
- **Response**: PromotionResponse

##### GET /api/promotion/active

- **Description**: Get active promotions only
- **Authorization**: Staff, Admin
- **Response**: IEnumerable<PromotionResponse>

##### GET /api/promotion/check-code/{promoCode}

- **Description**: Check if promo code exists
- **Authorization**: Staff, Admin
- **Parameters**: `promoCode` (promotion code)
- **Response**: boolean

##### POST /api/promotion/validate

- **Description**: Validate promotion with order amount
- **Authorization**: Staff, Admin
- **Body**: ValidatePromotionRequest
- **Response**: PromotionValidationResponse

##### POST /api/promotion/calculate-discount

- **Description**: Calculate discount amount
- **Authorization**: Staff, Admin
- **Body**: ValidatePromotionRequest
- **Response**: decimal (discount amount)

##### POST /api/promotion

- **Description**: Create new promotion
- **Authorization**: Admin only
- **Body**: CreatePromotionRequest
- **Response**: PromotionResponse (201 Created)

##### PUT /api/promotion/{id}

- **Description**: Update existing promotion
- **Authorization**: Admin only
- **Parameters**: `id` (promotion ID)
- **Body**: UpdatePromotionRequest
- **Response**: PromotionResponse

##### DELETE /api/promotion/{id}

- **Description**: Delete promotion
- **Authorization**: Admin only
- **Parameters**: `id` (promotion ID)
- **Response**: boolean

##### POST /api/promotion/deactivate-expired

- **Description**: Deactivate expired promotions
- **Authorization**: Admin only
- **Response**: boolean

## Key Features

### 1. Complex Business Logic

#### **Promotion Validation Engine:**

```csharp
// Comprehensive validation rules:
- Promotion status = "active"
- Current date within start_date and end_date
- Order amount >= minimum_order_amount
- Usage count < usage_limit (if limit set)
- Promo code format validation
- Business rule validation (start_date < end_date)
```

#### **Discount Calculation Engine:**

```csharp
public decimal CalculateDiscount(Promotion promotion, decimal orderAmount)
{
    if (promotion.DiscountType == DiscountType.Percent)
    {
        return orderAmount * (promotion.DiscountValue / 100);
    }
    else // Fixed
    {
        return Math.Min(promotion.DiscountValue, orderAmount);
    }
}
```

### 2. Usage Tracking & Management

#### **Real-time Usage Tracking:**

- Increment usage count when promotion is applied
- Check usage limits before applying
- Auto-deactivate when usage limit reached

#### **Time-based Management:**

- Background job to check expired promotions
- Auto-update status to "inactive" when expired
- Pre-validation before applying promotions

### 3. Advanced Validation

#### **Multi-level Validation:**

- Input validation (FluentValidation)
- Business rule validation
- Database constraint validation
- Runtime promotion validation

#### **Promotion Rules Engine:**

```csharp
// Validation scenarios:
- Promotion not found
- Promotion inactive
- Promotion expired
- Promotion not started yet
- Order amount too low
- Usage limit exceeded
```

### 4. Pagination (Controller Level)

#### **Efficient Pagination:**

- Service returns all matching promotions
- Controller applies pagination with Skip/Take
- Supports search and filtering
- Maximum page size limit of 100 items

### 5. Authorization & Security

#### **Role-based Access Control:**

- Staff and Admin: Read operations, validation
- Admin only: Create, Update, Delete operations
- JWT authentication required

### 6. Error Handling

#### **Comprehensive Error Handling:**

- Validation errors with detailed messages
- Business rule violations
- Database constraint violations
- Meaningful HTTP status codes

## Database Schema

### Promotions Table

```sql
CREATE TABLE promotions (
    promo_id INT PRIMARY KEY AUTO_INCREMENT,
    promo_code VARCHAR(50) UNIQUE NOT NULL,
    description VARCHAR(255),
    discount_type ENUM('percent','fixed') NOT NULL,
    discount_value DECIMAL(10,2) NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    min_order_amount DECIMAL(10,2) DEFAULT 0,
    usage_limit INT DEFAULT 0,
    used_count INT DEFAULT 0,
    status ENUM('active','inactive') DEFAULT 'active'
);
```

### Relationships

- Promotion has many Orders (one-to-many)
- Promotion affects Order total and discount amounts

## Validation Rules

### CreatePromotionRequest

- PromoCode: Required, max 50 chars, format validation
- Description: Optional, max 255 chars
- DiscountType: Required, must be DiscountType.Percent or DiscountType.Fixed
- DiscountValue: Required, > 0, max 100% for percent type
- StartDate: Required, cannot be in the past
- EndDate: Required, must be after StartDate
- MinOrderAmount: Optional, >= 0
- UsageLimit: Optional, >= 0

### UpdatePromotionRequest

- Same validation as CreatePromotionRequest
- Additional: UsedCount validation against UsageLimit

### ValidatePromotionRequest

- PromoCode: Required, max 50 chars
- OrderAmount: Required, > 0

## Business Logic Examples

### 1. Promotion Validation

```csharp
// Example validation flow:
1. Check if promotion exists
2. Validate promotion is active
3. Check date range (start_date <= now <= end_date)
4. Validate order amount >= min_order_amount
5. Check usage limit not exceeded
6. Return validation result with discount calculation
```

### 2. Discount Calculation

```csharp
// Example calculations:
- Percent discount: 10% on $100 = $10 discount
- Fixed discount: $50 off $100 = $50 discount
- Fixed discount: $50 off $30 = $30 discount (max = order amount)
```

### 3. Usage Tracking

```csharp
// Example usage flow:
1. Validate promotion before applying
2. Apply promotion to order
3. Increment used_count
4. Check if usage_limit reached
5. Auto-deactivate if limit exceeded
```

## API Usage Examples

### Create Promotion

```bash
POST /api/promotion
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "promoCode": "SUMMER2024",
  "description": "Summer sale 20% off",
  "discountType": 0,
  "discountValue": 20,
  "startDate": "2024-06-01",
  "endDate": "2024-08-31",
  "minOrderAmount": 100000,
  "usageLimit": 1000
}
```

### Validate Promotion

```bash
POST /api/promotion/validate
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "promoCode": "SUMMER2024",
  "orderAmount": 150000
}
```

### Get Promotions with Pagination

```bash
GET /api/promotion?pageNumber=1&pageSize=10&searchTerm=summer
Authorization: Bearer <jwt-token>
```

## Performance Considerations

### 1. Caching Strategy

- Cache active promotions for faster validation
- Cache promotion validation results
- Invalidate cache on promotion updates

### 2. Database Optimization

- Index on promo_code for fast lookups
- Index on start_date, end_date for date range queries
- Index on status for active promotion filters

### 3. Background Jobs

- Scheduled job to deactivate expired promotions
- Usage count updates can be batched
- Analytics can be processed asynchronously

## Future Enhancements

1. **Advanced Promotion Types:**

   - Category-specific promotions
   - Product-specific promotions
   - Customer-segment promotions

2. **Promotion Stacking:**

   - Multiple promotion combinations
   - Priority-based application
   - Stacking rules engine

3. **Advanced Analytics:**

   - Promotion performance metrics
   - ROI calculations
   - Customer behavior analysis

4. **Automated Promotion Management:**
   - AI-driven promotion suggestions
   - Dynamic pricing based on demand
   - Automated A/B testing

## Conclusion

The Promotion API implementation provides a robust foundation for promotion management with complex business logic, comprehensive validation, and efficient performance. The architecture supports future enhancements while maintaining clean separation of concerns and testability.

Key strengths:

- ✅ Complex business logic implementation
- ✅ Comprehensive validation and error handling
- ✅ Efficient pagination at controller level
- ✅ Role-based authorization
- ✅ Usage tracking and time-based management
- ✅ Discount calculation engine
- ✅ Clean architecture with proper separation of concerns
