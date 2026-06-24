# Final API Endpoints - GET Only

Since the Merchant System handles all CRUD operations (Add, Edit, Delete), this API now provides only GET endpoints for Categories and Products.

## ?? Category Endpoints

### 1. Get All Categories (with pagination and filtering)
```http
GET /api/Category?pageNumber=1&pageSize=10&searchTerm=electronics&merchantId=guid&isActive=true
```

**Response:**
```json
{
  "success": true,
  "message": "Categories retrieved successfully",
  "data": {
    "items": [
      {
        "id": "guid",
        "name": "Electronics",
        "description": "Electronic products",
        "isActive": true,
        "merchantID": "guid",
        "subCategories": [...]
      }
    ],
    "totalCount": 100,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 10,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

### 2. Get Category by ID
```http
GET /api/Category/{id:guid}
```

### 3. Get Root Categories
```http
GET /api/Category/root
```

### 4. Get Categories by Parent
```http
GET /api/Category/by-parent/{parentId:guid}
```

### 5. Get Categories by Merchant
```http
GET /api/Category/by-merchant/{merchantId:guid}
```

## ??? Product Endpoints

### 1. Get Product by ID
```http
GET /api/Product/{id:guid}
```

**Response:**
```json
{
  "productId": "guid",
  "productName": "iPhone 15",
  "description": "Latest iPhone model",
  "productDescription": "Detailed description...",
  "price": 999.99,
  "discount": 50.00,
  "stockQuantity": 100,
  "sku": "IPH15-001",
  "categoryId": "guid",
  "categoryName": "Smartphones",
  "subCategoryId": "guid",
  "subCategoryName": "Apple",
  "productSpecification": "JSON specification",
  "features": "Key features",
  "boxContents": "What's in the box",
  "productType": "Electronics",
  "isActive": true,
  "isFeatured": false,
  "status": "approved",
  "imageUrls": ["url1", "url2"],
  "merchantID": "guid",
  "createdOn": "2024-01-01T00:00:00Z",
  "createdBy": "admin"
}
```

### 2. Search All Products (with filtering and pagination)
```http
POST /api/Product/search
Content-Type: application/json

{
  "pageNumber": 1,
  "pageSize": 10,
  "searchTerm": "phone",
  "categoryId": "guid",
  "subCategoryId": "guid",
  "merchantId": "guid",
  "isActive": true,
  "isFeatured": true,
  "status": "approved",
  "minPrice": 100,
  "maxPrice": 1000,
  "inStock": true,
  "sortBy": "price",
  "sortOrder": "asc"
}
```

**Response:**
```json
{
  "items": [
    {
      "productId": "guid",
      "productName": "iPhone 15",
      "description": "Latest iPhone",
      "price": 999.99,
      "discount": 50.00,
      "stockQuantity": 100,
      "sku": "IPH15-001",
      "isActive": true,
      "isFeatured": false,
      "status": "approved",
      "imageUrls": ["url1"],
      "categoryName": "Smartphones",
      "subCategoryName": "Apple",
      "merchantID": "guid",
      "createdOn": "2024-01-01T00:00:00Z"
    }
  ],
  "totalCount": 50,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

### 3. Get Products by Category
```http
POST /api/Product/category/{categoryId:guid}
Content-Type: application/json

{
  "pageNumber": 1,
  "pageSize": 10,
  "searchTerm": "phone",
  "minPrice": 100,
  "maxPrice": 1000,
  "sortBy": "name",
  "sortOrder": "desc"
}
```

### 4. Legacy Endpoints (for backward compatibility)

#### Get All Products (Legacy)
```http
GET /api/Product/GetAllProducts
```

#### Get Products by Category (Legacy)
```http
POST /api/Product/GetProductsByCategory
Content-Type: application/json

{
  "categoryName": "Electronics",
  "categoryID": 1
}
```

## ?? Filter Options for Products

### Search and Filtering
- **searchTerm**: Search in product name, description, and product description
- **categoryId**: Filter by main category (Guid)
- **subCategoryId**: Filter by subcategory (Guid) 
- **subSubCategoryId**: Filter by sub-subcategory (Guid)
- **merchantId**: Filter by merchant (Guid)

### Status Filters
- **isActive**: Filter active/inactive products (boolean)
- **isFeatured**: Filter featured products (boolean)
- **status**: Filter by approval status ("pending", "approved", "rejected")

### Price Range
- **minPrice**: Minimum price filter (decimal)
- **maxPrice**: Maximum price filter (decimal)

### Stock Filter
- **inStock**: Show only products with stock > 0 (boolean)

### Sorting Options
- **sortBy**: "name", "price", "createdOn", "stockQuantity"
- **sortOrder**: "asc" or "desc"

### Pagination
- **pageNumber**: Page number (default: 1)
- **pageSize**: Items per page (default: 10, max: 50)

## ?? Key Features

1. **Database-First Approach**: Direct database access instead of external API calls
2. **Proper Relationships**: Foreign keys and navigation properties
3. **Guid-Based IDs**: Modern UUID-based identification
4. **Comprehensive Filtering**: Multiple filter options for products
5. **Pagination**: Built-in pagination for all list endpoints
6. **AutoMapper Integration**: Automatic model-to-DTO mapping
7. **Legacy Support**: Backward compatibility with existing integrations
8. **Consistent Response Format**: Standardized API responses
9. **Logging**: Comprehensive logging for debugging and monitoring
10. **Error Handling**: Proper error handling and status codes

## ?? Integration Notes

- All CRUD operations (Create, Update, Delete) are handled by the **Merchant System**
- This API focuses only on **data retrieval** (GET operations)
- Legacy endpoints maintained for backward compatibility during migration
- New endpoints use modern REST practices with proper HTTP methods
- All endpoints return consistent response formats
- Proper error handling with meaningful status codes and messages