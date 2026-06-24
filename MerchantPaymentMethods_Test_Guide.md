# Test Script for MerchantPaymentMethods Functionality

## Steps to Fix the Database Issue:

1. **Run the SQL Script**: Execute `Create_MerchantPaymentMethods_Table.sql` in your PostgreSQL database
2. **Verify Creation**: Run `Verify_MerchantPaymentMethods.sql` to confirm the table was created correctly
3. **Test the API**: Use the test requests below

## Test API Endpoints

### 1. Get System Payment Methods
```http
GET /api/PaymentMethod/system
Authorization: Bearer {your-token}
```

### 2. Create a Merchant with Payment Methods
```http
POST /api/Merchant/register
Authorization: Bearer {your-token}
Content-Type: application/json

{
    "businessName": "Test Business",
    "businessRegistration": "REG123456",
    "taxId": "TAX789",
    "contactPerson": "John Doe", 
    "email": "test@example.com",
    "phone": "+254700000000",
    "address": "Test Address",
    "city": "Nairobi",
    "country": "Kenya",
    "businessType": "retail",
    "businessCategory": "electronics",
    "deliveryMethod": "both",
    "returnPolicy": true,
    "termsAndCondition": true,
    "paymentMethods": [
        {
            "paymentMethodId": 1,
            "paymentMethodName": "M-Pesa",
            "isEnabled": true,
            "configuration": "{\"accountNumber\":\"123456\",\"accountName\":\"Test Paybill\",\"phoneNumber\":\"0700000000\",\"merchantCode\":\"12345\"}"
        }
    ],
    "documents": [
        "https://example.com/document1.pdf"
    ]
}
```

### 3. Get Merchant Payment Methods
```http
GET /api/PaymentMethod/merchant/{merchantId}
Authorization: Bearer {your-token}
```

### 4. Add Payment Method to Merchant
```http
POST /api/PaymentMethod/merchant
Authorization: Bearer {your-token}
Content-Type: application/json

{
    "merchantId": "merchant-guid-here",
    "paymentMethodId": 2,
    "configuration": "{\"accountNumber\":\"12345\",\"accountName\":\"Test Bank\",\"bankCode\":\"001\"}",
    "isEnabled": true
}
```

## Common Issues and Solutions

### Issue: Table doesn't exist
**Solution**: Run the `Create_MerchantPaymentMethods_Table.sql` script

### Issue: Foreign key constraint errors
**Solution**: Ensure `Merchants` and `PaymentMethods` tables exist and have the correct primary keys

### Issue: Duplicate key errors
**Solution**: Check if you're trying to add the same payment method to a merchant twice

## Sample cURL Commands

```bash
# Get system payment methods
curl -X GET "http://localhost:5000/api/PaymentMethod/system" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Register merchant with payment methods
curl -X POST "http://localhost:5000/api/Merchant/register" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "businessName": "Test Business",
    "businessRegistration": "REG123456",
    "contactPerson": "John Doe",
    "email": "test@example.com",
    "phone": "+254700000000",
    "address": "Test Address",
    "city": "Nairobi", 
    "country": "Kenya",
    "businessType": "retail",
    "deliveryMethod": "both",
    "returnPolicy": true,
    "termsAndCondition": true,
    "paymentMethods": [{
      "paymentMethodId": 1,
      "isEnabled": true,
      "configuration": "{\"accountNumber\":\"123456\",\"merchantCode\":\"12345\"}"
    }]
  }'
```

## Database Verification Queries

```sql
-- Check if table exists
SELECT tablename FROM pg_catalog.pg_tables WHERE tablename = 'MerchantPaymentMethods';

-- Count records
SELECT COUNT(*) FROM "MerchantPaymentMethods";

-- View all merchant payment methods with details
SELECT 
    mpm."Id",
    m."BusinessName",
    pm."Name" as "PaymentMethod",
    mpm."IsEnabled",
    mpm."CreatedAt"
FROM "MerchantPaymentMethods" mpm
JOIN "Merchants" m ON mpm."MerchantId" = m."MerchantID"
JOIN "PaymentMethods" pm ON mpm."PaymentMethodId" = pm."PaymentMethodID";
```