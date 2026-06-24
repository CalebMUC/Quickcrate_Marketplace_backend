## Categories API Documentation

This document describes the Categories API endpoints that fetch data from the external Merchant Service.

### Base URL
```
https://your-api-base-url.com/api/categories
```

### Authentication
All endpoints require authentication via JWT Bearer token.

### Endpoints

#### 1. Get All Categories (Paginated)
```http
GET /api/categories?pageNumber=1&pageSize=10&searchTerm=electronics&isActive=true&sortBy=Name&sortOrder=asc
```

**Query Parameters:**
- `pageNumber` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 10)
- `searchTerm` (optional): Search term to filter categories
- `isActive` (optional): Filter by active status
- `parentId` (optional): Filter by parent category ID
- `sortBy` (optional): Sort field (default: "Name")
- `sortOrder` (optional): Sort direction - "asc" or "desc" (default: "asc")

**Response:**
```json
{
  "success": true,
  "message": "Categories retrieved successfully",
  "data": {
    "items": [
      {
        "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "name": "Electronics",
        "description": "Electronic devices and accessories",
        "slug": "electronics",
        "isActive": true,
        "sortOrder": 1,
        "merchantId": "merchant-guid",
        "parentId": null,
        "imageUrl": "https://example.com/image.jpg",
        "metaTitle": "Electronics Category",
        "metaDescription": "Browse our electronics collection",
        "productCount": 150,
        "createdOn": "2024-01-01T00:00:00Z",
        "updatedOn": "2024-01-01T00:00:00Z",
        "createdBy": "admin",
        "updatedBy": "admin",
        "subCategories": [
          {
            "subCategoryId": "guid",
            "name": "Mobile Phones",
            "description": "Smartphones and accessories",
            "slug": "mobile-phones",
            "isActive": true,
            "sortOrder": 1,
            "categoryId": "parent-category-guid",
            "merchantId": "merchant-guid",
            "imageUrl": "https://example.com/mobile.jpg",
            "productCount": 50,
            "createdOn": "2024-01-01T00:00:00Z",
            "updatedOn": null,
            "createdBy": "admin",
            "updatedBy": null,
            "subSubCategories": []
          }
        ]
      }
    ],
    "totalCount": 1,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  },
  "errors": []
}
```

#### 2. Get Category by ID
```http
GET /api/categories/{id}
```

**Parameters:**
- `id` (required): Category UUID

**Response:**
```json
{
  "success": true,
  "message": "Category retrieved successfully",
  "data": {
    "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Electronics",
    "description": "Electronic devices and accessories",
    "slug": "electronics",
    "isActive": true,
    "sortOrder": 1,
    "merchantId": "merchant-guid",
    "parentId": null,
    "imageUrl": "https://example.com/image.jpg",
    "metaTitle": "Electronics Category",
    "metaDescription": "Browse our electronics collection",
    "productCount": 150,
    "createdOn": "2024-01-01T00:00:00Z",
    "updatedOn": "2024-01-01T00:00:00Z",
    "createdBy": "admin",
    "updatedBy": "admin",
    "subCategories": []
  },
  "errors": []
}
```

#### 3. Get Root Categories
```http
GET /api/categories/root
```

**Response:**
```json
{
  "success": true,
  "message": "Root categories retrieved successfully",
  "data": [
    {
      "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Electronics",
      "slug": "electronics",
      "isActive": true,
      "productCount": 150,
      "subCategories": []
    }
  ],
  "errors": []
}
```

#### 4. Get Categories by Parent ID
```http
GET /api/categories/by-parent/{parentId}
```

**Parameters:**
- `parentId` (required): Parent category UUID

**Response:**
```json
{
  "success": true,
  "message": "Categories retrieved successfully",
  "data": [
    {
      "categoryId": "child-category-guid",
      "name": "Mobile Phones",
      "slug": "mobile-phones",
      "parentId": "parent-category-guid",
      "isActive": true,
      "productCount": 50,
      "subCategories": []
    }
  ],
  "errors": []
}
```

### Error Responses

#### 400 Bad Request
```json
{
  "success": false,
  "message": "Invalid category ID",
  "errors": ["The provided category ID is not valid."]
}
```

#### 404 Not Found
```json
{
  "success": false,
  "message": "Category with ID 3fa85f64-5717-4562-b3fc-2c963f66afa6 not found",
  "errors": []
}
```

#### 500 Internal Server Error
```json
{
  "success": false,
  "message": "An error occurred while retrieving categories",
  "errors": []
}
```

### Configuration

To configure the external merchant service endpoint, update your `appsettings.json`:

```json
{
  "ExternalServices": {
    "MerchantService": {
      "BaseUrl": "https://your-merchant-service.com",
      "ApiKey": "your-api-key",
      "Timeout": 30
    }
  }
}
```

### Dependencies

The Categories API requires:
- AutoMapper (for model mapping)
- HttpClientFactory (for external service calls)
- Entity Framework Core (for database access)
- ASP.NET Core Identity (for authentication)

### Implementation Notes

1. **External Service Integration**: The API fetches data from an external merchant service rather than local database
2. **Caching**: Consider implementing caching for frequently accessed categories
3. **Error Handling**: All external service failures are gracefully handled with appropriate error responses
4. **Mapping**: AutoMapper is used to map between external models and API DTOs
5. **Authentication**: All endpoints require valid JWT authentication
6. **Logging**: Comprehensive logging is implemented for debugging and monitoring