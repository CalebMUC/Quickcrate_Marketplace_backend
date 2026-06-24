# Payout System Testing Guide

## Test Environment Setup

### Prerequisites
1. Run database migration: `Database_Migration_Payout_System.sql`
2. Ensure test merchants exist with completed orders
3. Configure payment methods for test merchants
4. Set up admin and merchant test users

## Test Data Setup

### Sample SQL for Test Data
```sql
-- Create test merchant (if not exists)
INSERT INTO "Merchants" ("MerchantID", "BusinessName", "MerchantName", "Email", "Phone", "Status", "RegistrationDate")
VALUES 
    ('aa8b7594-c9ce-4924-9d49-0f690ba84872', 'Victor Auto Spares', 'Victor Mwangi', 'quickcrate2@gmail.com', '+254714262062', 'Approved', NOW())
ON CONFLICT ("MerchantID") DO NOTHING;

-- Create test orders (modify as needed)
INSERT INTO "Orders" ("OrderID", "OrderedBy", "TotalPaymentAmount", "Status", "OrderDate")
VALUES 
    ('ORD001', 'John Doe', 1500.00, 'Completed', '2024-01-15 10:00:00+00'),
    ('ORD002', 'Jane Smith', 2500.00, 'Delivered', '2024-01-16 14:00:00+00'),
    ('ORD003', 'Bob Johnson', 3000.00, 'Completed', '2024-01-17 16:00:00+00')
ON CONFLICT ("OrderID") DO NOTHING;

-- Create test order products linking to merchant
INSERT INTO "OrderProducts" ("ProductId", "OrderId", "MerchantID", "Quantity", "TotalPrice")
VALUES 
    ('prod1', 'ORD001', 'aa8b7594-c9ce-4924-9d49-0f690ba84872', 3, 1500.00),
    ('prod2', 'ORD002', 'aa8b7594-c9ce-4924-9d49-0f690ba84872', 2, 2500.00),
    ('prod3', 'ORD003', 'aa8b7594-c9ce-4924-9d49-0f690ba84872', 1, 3000.00)
ON CONFLICT DO NOTHING;
```

## API Testing Scenarios

### 1. Admin Payout Generation

#### Test Case: Generate Weekly Payouts
```bash
# Generate payouts for test period
curl -X POST "http://localhost:5000/api/payout/admin/generate" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "periodStartDate": "2024-01-15T00:00:00Z",
    "periodEndDate": "2024-01-17T23:59:59Z",
    "processImmediately": false,
    "notes": "Test weekly payout generation"
  }'
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Successfully generated 1 payouts.",
  "data": {
    "success": true,
    "message": "Successfully generated 1 payouts.",
    "payoutsGenerated": 1,
    "totalGrossAmount": 7000.00,
    "totalCommissionAmount": 350.00,
    "totalNetAmount": 6650.00,
    "generatedPayouts": [
      {
        "payoutId": "generated-uuid",
        "merchantId": "aa8b7594-c9ce-4924-9d49-0f690ba84872",
        "grossAmount": 7000.00,
        "commissionAmount": 350.00,
        "netAmount": 6650.00,
        "status": "Pending",
        "orderCount": 3,
        "productCount": 3
      }
    ]
  }
}
```

#### Test Case: Generate with No Eligible Orders
```bash
curl -X POST "http://localhost:5000/api/payout/admin/generate" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "periodStartDate": "2023-01-01T00:00:00Z",
    "periodEndDate": "2023-01-07T23:59:59Z"
  }'
```

**Expected Response:**
```json
{
  "success": true,
  "message": "No eligible orders found for the specified period.",
  "data": {
    "success": true,
    "message": "No eligible orders found for the specified period.",
    "payoutsGenerated": 0,
    "totalGrossAmount": 0,
    "totalCommissionAmount": 0,
    "totalNetAmount": 0
  }
}
```

### 2. Merchant Payout Statistics

#### Test Case: Get Merchant Stats
```bash
curl -X GET "http://localhost:5000/api/payout/stats" \
  -H "Authorization: Bearer $MERCHANT_TOKEN"
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Payout statistics retrieved successfully",
  "data": {
    "totalEarnings": 7000.00,
    "pendingAmount": 6650.00,
    "completedAmount": 0.00,
    "totalCommissionPaid": 350.00,
    "totalPayouts": 1,
    "pendingPayouts": 1,
    "completedPayouts": 0,
    "averagePayoutAmount": 6650.00,
    "lastPayoutDate": null,
    "nextScheduledPayoutDate": null
  }
}
```

### 3. Admin Payout Management

#### Test Case: Get All Payouts (Admin)
```bash
curl -X GET "http://localhost:5000/api/payout/admin/all?page=1&pageSize=10" \
  -H "Authorization: Bearer $ADMIN_TOKEN"
```

#### Test Case: Update Payout Status
```bash
# First, get a payout ID from the generate response or list endpoint
PAYOUT_ID="your-payout-id-here"

# Update to Scheduled
curl -X PATCH "http://localhost:5000/api/payout/admin/$PAYOUT_ID/status" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Scheduled",
    "notes": "Scheduled for next business day"
  }'
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Payout status updated successfully"
}
```

#### Test Case: Mark as Completed
```bash
curl -X PATCH "http://localhost:5000/api/payout/admin/$PAYOUT_ID/status" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Completed",
    "externalPaymentReference": "BANK_TXN_123456",
    "notes": "Manual bank transfer completed successfully"
  }'
```

### 4. Merchant Payout Viewing

#### Test Case: Get Merchant Payouts
```bash
curl -X GET "http://localhost:5000/api/payout?page=1&pageSize=10" \
  -H "Authorization: Bearer $MERCHANT_TOKEN"
```

#### Test Case: Get Specific Payout Details
```bash
curl -X GET "http://localhost:5000/api/payout/$PAYOUT_ID" \
  -H "Authorization: Bearer $MERCHANT_TOKEN"
```

#### Test Case: Get Payout Transactions
```bash
curl -X GET "http://localhost:5000/api/payout/transactions?page=1&pageSize=10" \
  -H "Authorization: Bearer $MERCHANT_TOKEN"
```

### 5. Error Testing

#### Test Case: Invalid Status Transition
```bash
# Try to mark completed payout as pending (should fail)
curl -X PATCH "http://localhost:5000/api/payout/admin/$COMPLETED_PAYOUT_ID/status" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Pending"
  }'
```

**Expected Response:**
```json
{
  "success": false,
  "message": "Failed to update payout status. Please check the payout exists and status transition is valid."
}
```

#### Test Case: Merchant Access to Other Merchant's Payout
```bash
curl -X GET "http://localhost:5000/api/payout/$OTHER_MERCHANT_PAYOUT_ID" \
  -H "Authorization: Bearer $MERCHANT_TOKEN"
```

**Expected Response:**
```json
{
  "success": false,
  "message": "Payout not found"
}
```

#### Test Case: Generate Payouts Without Admin Role
```bash
curl -X POST "http://localhost:5000/api/payout/admin/generate" \
  -H "Authorization: Bearer $MERCHANT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "periodStartDate": "2024-01-15T00:00:00Z",
    "periodEndDate": "2024-01-17T23:59:59Z"
  }'
```

**Expected Response:** 403 Forbidden

## Integration Testing

### Test Case: Full Payout Lifecycle
```bash
#!/bin/bash

# 1. Generate payouts (Admin)
echo "1. Generating payouts..."
GENERATE_RESPONSE=$(curl -s -X POST "http://localhost:5000/api/payout/admin/generate" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "periodStartDate": "2024-01-15T00:00:00Z",
    "periodEndDate": "2024-01-17T23:59:59Z"
  }')

# Extract payout ID (assuming jq is available)
PAYOUT_ID=$(echo $GENERATE_RESPONSE | jq -r '.data.generatedPayouts[0].payoutId')
echo "Generated Payout ID: $PAYOUT_ID"

# 2. Verify merchant can see the payout
echo "2. Checking merchant can see payout..."
curl -s -X GET "http://localhost:5000/api/payout/$PAYOUT_ID" \
  -H "Authorization: Bearer $MERCHANT_TOKEN" | jq '.success'

# 3. Schedule the payout (Admin)
echo "3. Scheduling payout..."
curl -s -X PATCH "http://localhost:5000/api/payout/admin/$PAYOUT_ID/status" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"status": "Scheduled"}'

# 4. Mark as processing (Admin)
echo "4. Marking as processing..."
curl -s -X PATCH "http://localhost:5000/api/payout/admin/$PAYOUT_ID/status" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"status": "Processing"}'

# 5. Complete the payout (Admin)
echo "5. Completing payout..."
curl -s -X PATCH "http://localhost:5000/api/payout/admin/$PAYOUT_ID/status" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Completed",
    "externalPaymentReference": "BANK_TXN_123456"
  }'

# 6. Verify merchant stats updated
echo "6. Checking updated merchant stats..."
curl -s -X GET "http://localhost:5000/api/payout/stats" \
  -H "Authorization: Bearer $MERCHANT_TOKEN" | jq '.data.completedAmount'

echo "Payout lifecycle test completed!"
```

## Performance Testing

### Load Test Scenarios

#### Test Case: Multiple Concurrent Payout Generations
```bash
# Generate multiple payouts for different periods simultaneously
for i in {1..5}; do
  (curl -X POST "http://localhost:5000/api/payout/admin/generate" \
    -H "Authorization: Bearer $ADMIN_TOKEN" \
    -H "Content-Type: application/json" \
    -d "{
      \"periodStartDate\": \"2024-01-0${i}T00:00:00Z\",
      \"periodEndDate\": \"2024-01-0${i}T23:59:59Z\"
    }" &)
done
wait
```

#### Test Case: High-Volume Payout Queries
```bash
# Simulate multiple merchants checking stats simultaneously
for i in {1..10}; do
  (curl -X GET "http://localhost:5000/api/payout/stats" \
    -H "Authorization: Bearer $MERCHANT_TOKEN_$i" &)
done
wait
```

## Database Testing

### Verify Data Integrity
```sql
-- Check payout calculations
SELECT 
    p."PayoutId",
    p."GrossAmount",
    p."CommissionAmount",
    p."NetAmount",
    p."CommissionRate",
    SUM(pt."OrderAmount") as calculated_gross,
    SUM(pt."CommissionAmount") as calculated_commission,
    SUM(pt."NetAmount") as calculated_net
FROM "Payouts" p
JOIN "PayoutTransactions" pt ON p."PayoutId" = pt."PayoutId"
GROUP BY p."PayoutId", p."GrossAmount", p."CommissionAmount", p."NetAmount", p."CommissionRate"
HAVING 
    ABS(p."GrossAmount" - SUM(pt."OrderAmount")) > 0.01 OR
    ABS(p."CommissionAmount" - SUM(pt."CommissionAmount")) > 0.01 OR
    ABS(p."NetAmount" - SUM(pt."NetAmount")) > 0.01;

-- Verify no duplicate orders in payouts
SELECT 
    "OrderId",
    COUNT(*) as payout_count
FROM "PayoutTransactions"
GROUP BY "OrderId"
HAVING COUNT(*) > 1;

-- Check commission rate consistency
SELECT 
    p."PayoutId",
    p."CommissionRate" as payout_rate,
    pt."CommissionRate" as transaction_rate
FROM "Payouts" p
JOIN "PayoutTransactions" pt ON p."PayoutId" = pt."PayoutId"
WHERE ABS(p."CommissionRate" - pt."CommissionRate") > 0.0001;
```

## Troubleshooting Common Issues

### Issue 1: Orders Not Appearing in Payouts
**Check:**
```sql
-- Verify order status
SELECT "OrderID", "Status", "OrderDate" FROM "Orders" WHERE "OrderID" = 'your-order-id';

-- Check if already in payout
SELECT pt."OrderId", p."PayoutId", p."Status" 
FROM "PayoutTransactions" pt
JOIN "Payouts" p ON pt."PayoutId" = p."PayoutId"
WHERE pt."OrderId" = 'your-order-id';
```

### Issue 2: Commission Calculations Wrong
**Check:**
```sql
-- Verify commission calculation
SELECT 
    "OrderAmount",
    "CommissionRate",
    "CommissionAmount",
    "OrderAmount" * "CommissionRate" as expected_commission
FROM "PayoutTransactions"
WHERE ABS("CommissionAmount" - ("OrderAmount" * "CommissionRate")) > 0.01;
```

### Issue 3: Status Transition Errors
**Check logs for:**
- Invalid status transition attempts
- User permission issues
- Payout not found errors

## Test Results Documentation

### Template for Test Results
```markdown
## Test Execution Report

**Date:** [Date]
**Environment:** [Development/Staging]
**Tester:** [Name]

### Test Results Summary
- ? Payout Generation: PASS
- ? Status Updates: PASS
- ? Merchant Access: PASS
- ? Admin Operations: PASS
- ? Error Handling: PASS
- ? Data Integrity: PASS

### Issues Found
1. [Issue description] - [Severity] - [Status]

### Performance Results
- Average payout generation time: [X] seconds
- Concurrent request handling: [X] requests/second
- Database query performance: [X] ms average

### Recommendations
- [Any recommendations for improvements]
```

This comprehensive testing guide ensures all aspects of the payout system are thoroughly validated before production deployment.