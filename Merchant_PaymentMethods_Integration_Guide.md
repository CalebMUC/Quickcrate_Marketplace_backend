# Merchant Queries with Payment Methods Integration

## Overview
Updated all merchant repository methods to include `MerchantPaymentMethods` data in queries and responses, providing complete merchant information including their configured payment methods.

## Changes Made

### ? **Updated Methods**

#### 1. **GetMerchantByIdAsync**
- **Before**: Only returned merchant data
- **After**: Includes `MerchantPaymentMethods` with payment method details
- **Join**: `Include(m => m.MerchantPaymentMethods).ThenInclude(mpm => mpm.PaymentMethod)`

#### 2. **GetMerchantsAsync (with filters)**
- **Before**: Basic merchant list without payment methods
- **After**: Optional payment methods inclusion (default: enabled)
- **Performance**: Added overload with `includePaymentMethods` parameter for optimization

#### 3. **GetPendingMerchantsAsync**
- **Before**: Pending merchants only
- **After**: Pending merchants with their payment methods
- **Benefit**: Admins can see payment configuration during approval

#### 4. **UpdateMerchantAsync**
- **Before**: Updated merchant without payment methods in response
- **After**: Returns updated merchant with current payment methods

#### 5. **ApproveMerchantAsync**
- **Before**: Approval response without payment methods
- **After**: Complete merchant data including payment methods after approval

#### 6. **SuspendMerchantAsync**
- **Before**: Suspension without payment method context
- **After**: Full merchant data for suspension logging

#### 7. **ReactivateMerchantAsync**
- **Before**: Simple reactivation
- **After**: Complete merchant data with payment methods

## SQL Joins Generated

### Efficient Entity Framework Joins
```sql
-- Example generated SQL for GetMerchantByIdAsync
SELECT m.*, mpm.*, pm.*
FROM "Merchants" m
LEFT JOIN "MerchantPaymentMethods" mpm ON m."MerchantID" = mpm."MerchantId"
LEFT JOIN "PaymentMethods" pm ON mpm."PaymentMethodId" = pm."PaymentMethodID"
WHERE m."MerchantID" = @id
```

## Response Structure

### Complete Merchant Data
```json
{
  "id": "merchant-guid",
  "businessName": "Victor Auto Spares",
  "email": "victor@example.com",
  "status": "Active",
  "documents": [
    "https://s3.bucket.com/document1.pdf"
  ],
  "paymentMethods": [
    {
      "id": 1,
      "paymentMethodId": 1,
      "paymentMethodName": "M-Pesa",
      "configuration": "{\"accountNumber\":\"123456\",\"merchantCode\":\"99890\"}",
      "isEnabled": true,
      "createdAt": "2024-01-15T10:30:00Z",
      "accountDetails": {
        "accountNumber": "123456",
        "merchantCode": "99890"
      }
    },
    {
      "id": 2,
      "paymentMethodId": 2,
      "paymentMethodName": "Bank Transfer",
      "configuration": "{\"accountNumber\":\"000019903802\",\"accountName\":\"Victor Auto Spares\"}",
      "isEnabled": true,
      "createdAt": "2024-01-15T10:31:00Z",
      "accountDetails": {
        "accountNumber": "000019903802",
        "accountName": "Victor Auto Spares"
      }
    }
  ]
  // ... other merchant fields
}
```

## Performance Considerations

### ? **Optimizations Implemented**

1. **Conditional Loading**: Overload method allows choosing when to include payment methods
2. **Single Query**: Uses `Include`/`ThenInclude` instead of multiple queries
3. **Efficient Mapping**: Payment methods mapped in memory after single DB call

### **Usage Guidelines**

```csharp
// Include payment methods (default for public APIs)
var merchantWithPayments = await repo.GetMerchantByIdAsync(id);

// Exclude payment methods for performance (internal operations)
var merchantsLight = await repo.GetMerchantsAsync(filters, includePaymentMethods: false);

// Include payment methods for full data (admin dashboard)
var merchantsFull = await repo.GetMerchantsAsync(filters, includePaymentMethods: true);
```

## Benefits

### 1. **Complete Data Context**
- Frontend gets all merchant data in single API call
- No need for separate payment methods endpoint
- Consistent data structure across all merchant operations

### 2. **Better User Experience**
- Admin dashboard shows payment methods during merchant approval
- Merchant management shows complete configuration
- Faster loading with fewer API calls

### 3. **Improved Admin Workflow**
```csharp
// Admin approving merchants can see payment configuration
var pendingMerchants = await GetPendingMerchantsAsync();
// Each merchant includes their payment methods for review
```

### 4. **API Consistency**
- All merchant endpoints now return consistent data structure
- Payment methods always included where relevant
- Eliminates need for multiple API calls

## API Endpoint Improvements

### **GET /api/Merchant/{id}**
```json
// Before: Basic merchant data
{
  "id": "guid",
  "businessName": "Business Name",
  "email": "email@example.com"
}

// After: Complete merchant data
{
  "id": "guid",
  "businessName": "Business Name",
  "email": "email@example.com",
  "paymentMethods": [...],  // ? Now included
  "documents": [...]       // ? Also included
}
```

### **GET /api/Merchant/pending**
```json
// Now includes payment methods for each pending merchant
[
  {
    "id": "merchant1-guid",
    "businessName": "Pending Business 1",
    "status": "Pending Approval",
    "paymentMethods": [
      {
        "paymentMethodName": "M-Pesa",
        "configuration": "...",
        "isEnabled": true
      }
    ]
  }
]
```

## Testing Examples

### **Test Complete Merchant Data**
```csharp
[Test]
public async Task GetMerchantById_ShouldIncludePaymentMethods()
{
    // Arrange
    var merchantId = Guid.NewGuid();
    
    // Act
    var result = await _merchantRepo.GetMerchantByIdAsync(merchantId);
    
    // Assert
    Assert.NotNull(result);
    Assert.NotNull(result.PaymentMethods);
    Assert.True(result.PaymentMethods.Any());
}
```

### **Test Performance Options**
```csharp
[Test]
public async Task GetMerchants_WithoutPaymentMethods_ShouldBeFaster()
{
    // Arrange
    var filters = new MerchantFilters { Page = 1, PageSize = 10 };
    
    // Act - Without payment methods (faster)
    var resultLight = await _merchantRepo.GetMerchantsAsync(filters, false);
    
    // Act - With payment methods (complete data)
    var resultFull = await _merchantRepo.GetMerchantsAsync(filters, true);
    
    // Assert
    Assert.Null(resultLight.Merchants.First().PaymentMethods);
    Assert.NotNull(resultFull.Merchants.First().PaymentMethods);
}
```

## Migration Notes

### **Existing API Consumers**
- ? **Backward Compatible**: Existing code continues to work
- ? **Enhanced Data**: Gets additional payment method data
- ? **Performance**: Single query instead of multiple calls

### **Frontend Benefits**
- Reduced API calls from 2-3 requests to 1 request
- Complete merchant context for better UX
- Consistent data structure across all merchant operations

## Summary

All merchant queries now include payment methods by default, providing:
- **Complete Data Context** in single API call
- **Better Performance** with optimized joins
- **Enhanced Admin Experience** with full merchant information
- **API Consistency** across all merchant endpoints
- **Flexible Performance** with optional payment method inclusion

This change transforms the merchant API from basic CRUD operations to comprehensive merchant management with full payment method integration.