# Payout System Implementation Guide

## Overview
This document provides a comprehensive guide for the newly implemented Payout Backend Architecture, designed for manual payouts with future automation capabilities.

## ??? System Architecture

### Database Entities

#### 1. **Payout** (Main Aggregate Entity)
- **Purpose**: Represents a payout period per merchant
- **Key Features**:
  - Tracks gross, commission, and net amounts
  - Manages payout status lifecycle
  - Links to merchant and payment methods
  - Aggregates order and product counts

#### 2. **PayoutTransaction** (Transaction Detail Entity)
- **Purpose**: Individual orders included in each payout
- **Key Features**:
  - Links orders to payouts
  - Tracks per-order commission calculations
  - Maintains audit trail
  - Prevents duplicate order inclusion

## ?? Payout Status Lifecycle

```
Pending ? Scheduled ? Processing ? Completed
                              ?
                           Failed
                              ?
                        (Can retry)
```

### Status Definitions:
- **Pending**: Generated, awaiting manual scheduling
- **Scheduled**: Payment date assigned by admin
- **Processing**: Payment execution in progress
- **Completed**: Funds successfully transferred
- **Failed**: Payment failed (with reason logged)
- **Cancelled**: Admin-cancelled payout

## ?? Commission Calculation

### Default Settings:
- **Commission Rate**: 5% (configurable)
- **Calculation**: 
  - `Gross Amount = Sum of all order totals`
  - `Commission = Gross Amount × Commission Rate`
  - `Net Amount = Gross Amount - Commission`

### Multi-level Calculation:
- **Payout Level**: Summary commission for entire payout
- **Transaction Level**: Per-order commission tracking

## ?? Security & Authorization

### Access Control:
- **Merchants**: Read-only access to own payouts
- **Admins**: Full access + generation/status updates

### Authentication:
- JWT-based authentication required for all endpoints
- Merchant ID automatically resolved from token claims

## ?? API Endpoints

### Merchant Endpoints

#### **GET /api/payout/stats**
Get payout statistics for current merchant
```json
{
  "success": true,
  "data": {
    "totalEarnings": 15000.00,
    "pendingAmount": 3000.00,
    "completedAmount": 12000.00,
    "totalCommissionPaid": 750.00,
    "totalPayouts": 8,
    "pendingPayouts": 2,
    "completedPayouts": 6,
    "averagePayoutAmount": 1875.00,
    "lastPayoutDate": "2024-01-15T10:30:00Z",
    "nextScheduledPayoutDate": "2024-01-22T10:00:00Z"
  }
}
```

#### **GET /api/payout**
Get paginated payouts with filters
```
Query Parameters:
- status: Filter by payout status
- startDate: Period start filter
- endDate: Period end filter
- page: Page number (default: 1)
- pageSize: Items per page (default: 20)
```

#### **GET /api/payout/{id}**
Get detailed payout information including all transactions

#### **GET /api/payout/transactions**
Get individual payout transactions with pagination

### Admin Endpoints

#### **GET /api/payout/admin/all**
Get all payouts with comprehensive filtering
```
Query Parameters:
- merchantId: Filter by specific merchant
- status: Filter by status
- startDate/endDate: Date range filters
- minAmount/maxAmount: Amount range filters
- page/pageSize: Pagination
- sortBy/sortOrder: Sorting options
```

#### **POST /api/payout/admin/generate**
Generate payouts for a period
```json
{
  "periodStartDate": "2024-01-01T00:00:00Z",
  "periodEndDate": "2024-01-07T23:59:59Z",
  "processImmediately": false,
  "merchantIds": ["optional-merchant-filter"],
  "commissionRate": 0.05,
  "notes": "Weekly payout generation"
}
```

#### **PATCH /api/payout/admin/{id}/status**
Update payout status
```json
{
  "status": "Completed",
  "reason": "Payment processed successfully",
  "externalPaymentReference": "PAY123456",
  "notes": "Manual bank transfer completed"
}
```

#### **GET /api/payout/admin/merchant-summaries**
Get merchant payout summaries for dashboard

### Utility Endpoints

#### **GET /api/payout/statuses**
Get available payout statuses with descriptions

## ?? Implementation Steps

### 1. Database Setup
```sql
-- Run the migration script
\i Database_Migration_Payout_System.sql
```

### 2. Service Registration
Services are automatically registered in `Program.cs`:
```csharp
builder.Services.AddScoped<IPayoutService, PayoutService>();
```

### 3. Testing the Implementation

#### Generate Test Payouts (Admin)
```bash
curl -X POST "https://yourapi.com/api/payout/admin/generate" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "periodStartDate": "2024-01-01T00:00:00Z",
    "periodEndDate": "2024-01-07T23:59:59Z",
    "processImmediately": false
  }'
```

#### Check Merchant Stats
```bash
curl -X GET "https://yourapi.com/api/payout/stats" \
  -H "Authorization: Bearer YOUR_MERCHANT_TOKEN"
```

#### Update Payout Status (Admin)
```bash
curl -X PATCH "https://yourapi.com/api/payout/admin/{payoutId}/status" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Completed",
    "externalPaymentReference": "BANK_REF_123"
  }'
```

## ?? Business Logic

### Payout Generation Process

1. **Order Eligibility Check**:
   - Only "Completed" or "Delivered" orders
   - Within specified date range
   - Not already included in previous payouts

2. **Merchant Grouping**:
   - Orders grouped by merchant
   - Individual calculations per merchant

3. **Amount Calculations**:
   - Gross: Sum of all order amounts
   - Commission: Gross × Commission Rate
   - Net: Gross - Commission

4. **Payment Method Verification**:
   - Check merchant has enabled payment method
   - Link payout to merchant's primary method

5. **Transaction Recording**:
   - Create payout record
   - Create individual transaction records per order
   - Set initial status (Pending/Scheduled)

### Validation Rules

- **Status Transitions**: Only valid transitions allowed
- **Order Uniqueness**: Orders can only be in one payout
- **Amount Validation**: All amounts must be non-negative
- **Date Validation**: End date must be after start date
- **Payment Method**: Must exist for payout completion

## ?? Monitoring & Analytics

### Key Metrics to Track

1. **Platform Metrics**:
   - Total payouts generated
   - Total commission earned
   - Average payout processing time
   - Success/failure rates

2. **Merchant Metrics**:
   - Individual merchant earnings
   - Payout frequency
   - Commission rates
   - Payment method preferences

3. **Operational Metrics**:
   - Manual intervention frequency
   - Processing delays
   - Failed payment reasons

### Sample Analytics Queries

```sql
-- Monthly payout summary
SELECT 
    DATE_TRUNC('month', "CreatedDate") as month,
    COUNT(*) as total_payouts,
    SUM("GrossAmount") as total_gross,
    SUM("CommissionAmount") as total_commission,
    SUM("NetAmount") as total_net
FROM "Payouts"
GROUP BY DATE_TRUNC('month', "CreatedDate")
ORDER BY month DESC;

-- Merchant performance
SELECT 
    m."BusinessName",
    COUNT(p."PayoutId") as payout_count,
    AVG(p."NetAmount") as avg_payout,
    MAX(p."CompletedDate") as last_payout
FROM "Merchants" m
LEFT JOIN "Payouts" p ON m."MerchantID" = p."MerchantId"
WHERE p."Status" = 'Completed'
GROUP BY m."MerchantID", m."BusinessName"
ORDER BY avg_payout DESC;
```

## ??? Troubleshooting

### Common Issues

1. **Payout Generation Fails**:
   - Check merchant payment method configuration
   - Verify order status and dates
   - Check for duplicate order inclusions

2. **Status Update Fails**:
   - Validate status transition rules
   - Check user permissions
   - Verify payout exists

3. **Commission Calculations**:
   - Verify commission rate configuration
   - Check order amount calculations
   - Validate decimal precision

### Debug Endpoints

Enable detailed logging for troubleshooting:
- Service operations are logged with correlation IDs
- Database operations include performance metrics
- API calls include request/response logging

## ?? Future Enhancements

### Automation Features (Planned)
1. **Scheduled Generation**: Automated weekly/monthly payouts
2. **Payment Gateway Integration**: Direct bank transfers
3. **Real-time Notifications**: Merchant payout alerts
4. **Advanced Analytics**: Revenue forecasting
5. **Multi-currency Support**: International payments
6. **Batch Processing**: High-volume merchant support

### Integration Points
- **Banking APIs**: Direct bank transfer automation
- **Notification Service**: Email/SMS alerts
- **Reporting System**: Advanced analytics dashboard
- **Audit System**: Compliance and transaction logging

## ?? Support

### For Implementation Issues
- Check service logs for detailed error information
- Verify database connectivity and permissions
- Confirm JWT token validity and permissions
- Validate merchant/admin role assignments

### For Business Logic Questions
- Review commission calculation formulas
- Confirm status transition rules
- Validate payout eligibility criteria
- Check payment method requirements

This implementation provides a solid foundation for manual payout management with built-in scalability for future automation features.