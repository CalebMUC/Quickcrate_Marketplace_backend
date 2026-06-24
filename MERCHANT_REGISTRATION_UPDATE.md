# Merchant Registration API Update

## Overview
Updated the merchant registration endpoint to accept JSON requests with payment method configurations and simplified document handling using a Documents array instead of individual certificate fields.

## Changes Made

### 1. Model Changes
- **File**: `Models/Merchants.cs`
- **Removed**: `KRAPINCertificate`, `BusinessRegistrationCertificate` fields
- **Added**: `Documents` property as `List<string>` configured as PostgreSQL text array
- **Benefit**: Flexible document storage supporting multiple document types

### 2. Database Configuration
- **File**: `Data/MinimartDBContext.cs`
- **Added**: Configuration for `Documents` field as PostgreSQL text array (`text[]`)
- **Migration**: See `Database_Migration_Documents_Array.sql`

### 3. Controller Changes
- **File**: `Controllers/MerchantController.cs`
- **Change**: Updated `RegisterMerchant` endpoint from `[FromForm]` to `[FromBody]` 
- **Line**: `public async Task<IActionResult> RegisterMerchant([FromBody] MerchantRegistrationDto dto)`

### 4. DTOs Updated
- **File**: `DTOS/Merchants/EnhancedMerchantDtos.cs`
- **Added**: 
  - `MerchantPaymentMethodRegistrationDto` - For payment method configuration during registration
  - `PaymentAccountDetailsDto` - For structured account details
  - `DocumentUrlDto` - For document URL information
  - `MerchantPaymentMethodDetailDto` - For response data
- **Updated**: 
  - `MerchantRegistrationDto.Documents` - Changed to `List<string>` for document URLs
  - `UpdateMerchantDto.Documents` - Added for updating documents

### 5. Response DTOs Updated
- **File**: `DTOS/Merchants/MerchantResponseDtos.cs`
- **Updated**: `MerchantDetailDto.Documents` - Changed from complex objects to simple `List<string>`
- **Added**: `PaymentMethods` property to `MerchantDetailDto`

### 6. Repository Changes
- **File**: `Repositories/Merchant/MerchantRepo.cs`
- **Updated**: All merchant queries and mapping methods to:
  - Handle Documents array instead of individual certificate fields
  - Save payment method configurations to `MerchantPaymentMethods` table
  - Use database transactions for data integrity
  - Remove references to old bank/payment fields

## New JSON Request Format

```json
{
    "businessName": "Victor Auto Spares",
    "businessRegistration": "000120202010", 
    "taxId": "P045647829T",
    "businessNature": "Online store",
    "businessType": "retail",
    "businessCategory": "electronics",
    "contactPerson": "Victor Mutuma",
    "email": "victormutuma788@gmail.com",
    "phone": "+254714262062",
    "address": "Nairobi,Kenya",
    "city": "Nairobi",
    "country": "Kenya",
    "socialMedia": "@vicmutumaauto",
    "deliveryMethod": "both",
    "returnPolicy": true,
    "termsAndCondition": true,
    "paymentMethods": [
        {
            "paymentMethodId": 2,
            "paymentMethodName": "Bank Transfer",
            "isEnabled": true,
            "configuration": "{\"accountNumber\":\"000019903802\",\"accountName\":\"Victor Auto Spares\",\"phoneNumber\":\"0714262062\",\"merchantCode\":\"008090\"}",
            "accountDetails": {
                "accountNumber": "000019903802",
                "accountName": "Victor Auto Spares",
                "phoneNumber": "0714262062",
                "merchantCode": "008090"
            }
        },
        {
            "paymentMethodId": 1,
            "paymentMethodName": "M-Pesa",
            "isEnabled": true,
            "configuration": "{\"accountNumber\":\"123456\",\"accountName\":\"Paybill\",\"phoneNumber\":\"0714262062\",\"merchantCode\":\"99890\"}",
            "accountDetails": {
                "accountNumber": "123456",
                "accountName": "Paybill", 
                "phoneNumber": "0714262062",
                "merchantCode": "99890"
            }
        }
    ],
    "documents": [
        "https://s3.bucket.com/documents/business-license-12345.pdf",
        "https://s3.bucket.com/documents/tax-certificate-67890.pdf",
        "https://s3.bucket.com/documents/id-copy-abc.jpg"
    ]
}
```

## Response Format
The response now includes the documents as URLs and configured payment methods:

```json
{
    "success": true,
    "message": "Merchant registered successfully",
    "data": {
        "id": "merchant-guid",
        "businessName": "Victor Auto Spares",
        "email": "victormutuma788@gmail.com",
        "documents": [
            "https://s3.bucket.com/documents/business-license-12345.pdf",
            "https://s3.bucket.com/documents/tax-certificate-67890.pdf"
        ],
        "paymentMethods": [
            {
                "id": 1,
                "paymentMethodId": 2,
                "paymentMethodName": "Bank Transfer", 
                "configuration": "{\"accountNumber\":\"000019903802\"...}",
                "isEnabled": true,
                "createdAt": "2024-01-15T10:30:00Z"
            }
        ]
        // ... other merchant details
    }
}
```

## Database Changes
- **Documents Storage**: Documents are now stored as PostgreSQL text array for better flexibility
- **Data Migration**: Existing certificate data can be migrated using the provided SQL script
- **Payment Methods**: Payment configurations stored in `MerchantPaymentMethods` junction table
- **Removed Fields**: Old certificate and bank fields removed from Merchants table

## Benefits
1. **Simplified Document Handling**: Single Documents array supports any number of document types
2. **URL-Based Storage**: Documents stored as URLs, compatible with cloud storage (S3, etc.)
3. **Better Data Structure**: Payment methods properly normalized
4. **JSON API**: Modern API design using JSON instead of form data
5. **Flexible Configuration**: Each payment method can have custom configuration
6. **Database Integrity**: Uses transactions to ensure data consistency
7. **Extensible**: Easy to add new document types and payment methods

## Migration Guide
1. Run the database migration script: `Database_Migration_Documents_Array.sql`
2. Update frontend code to send documents as array of URLs
3. Existing certificate data will be preserved during migration
4. Test with sample requests to ensure compatibility

## Document Upload Workflow
1. Upload documents to cloud storage (S3) using existing upload endpoints
2. Get the URLs from upload response
3. Include the URLs in the documents array during merchant registration
4. Documents are stored as references, actual files remain in cloud storage