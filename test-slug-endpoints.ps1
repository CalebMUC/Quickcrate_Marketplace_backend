# Test All Product Endpoints for Slug Property
# Run this in PowerShell or bash (modify URLs as needed)

$baseUrl = "http://localhost:5000"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Testing All Product Endpoints for Slug" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Function to test endpoint and check for slug
function Test-EndpointForSlug {
    param(
        [string]$Method,
        [string]$Url,
        [string]$Description,
        [object]$Body = $null
    )
    
    Write-Host "Testing: $Description" -ForegroundColor Yellow
    Write-Host "URL: $Method $Url" -ForegroundColor Gray
    
    try {
        $response = if ($Method -eq "POST") {
            Invoke-RestMethod -Uri $Url -Method Post -Body ($Body | ConvertTo-Json) -ContentType "application/json"
        } else {
            Invoke-RestMethod -Uri $Url -Method Get
        }
        
        # Check if response contains slug
        $hasSlug = $false
        $slugValue = $null
        
        if ($response.slug) {
            $hasSlug = $true
            $slugValue = $response.slug
        }
        elseif ($response.data.slug) {
            $hasSlug = $true
            $slugValue = $response.data.slug
        }
        elseif ($response.data -is [array] -and $response.data.Count -gt 0 -and $response.data[0].slug) {
            $hasSlug = $true
            $slugValue = $response.data[0].slug
        }
        elseif ($response.data.items -is [array] -and $response.data.items.Count -gt 0 -and $response.data.items[0].slug) {
            $hasSlug = $true
            $slugValue = $response.data.items[0].slug
        }
        
        if ($hasSlug) {
            Write-Host "? PASS - Slug found: $slugValue" -ForegroundColor Green
        } else {
            Write-Host "? FAIL - No slug property found in response" -ForegroundColor Red
            Write-Host "Response structure:" -ForegroundColor Gray
            Write-Host ($response | ConvertTo-Json -Depth 2) -ForegroundColor Gray
        }
    }
    catch {
        Write-Host "? ERROR - Failed to call endpoint: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    Write-Host ""
}

# Test 1: Get Product by ID
Write-Host "1?? Testing: Get Product by ID" -ForegroundColor Cyan
Test-EndpointForSlug -Method "GET" -Url "$baseUrl/api/Product/6949aa56-xxxx-xxxx-xxxx-xxxxxxxxxxxx" -Description "GET /api/Product/{id}"

# Test 2: Get Product (Enhanced)
Write-Host "2?? Testing: Get Product Enhanced" -ForegroundColor Cyan
Test-EndpointForSlug -Method "GET" -Url "$baseUrl/api/Product/GetProduct/6949aa56-xxxx-xxxx-xxxx-xxxxxxxxxxxx" -Description "GET /api/Product/GetProduct/{id}"

# Test 3: Get Product by Slug
Write-Host "3?? Testing: Get Product by Slug" -ForegroundColor Cyan
Test-EndpointForSlug -Method "GET" -Url "$baseUrl/api/Product/slug/getac-k120-core-i5-6949aa56" -Description "GET /api/Product/slug/{slug}"

# Test 4: Get Featured Products
Write-Host "4?? Testing: Get Featured Products" -ForegroundColor Cyan
Test-EndpointForSlug -Method "GET" -Url "$baseUrl/api/Product/GetFeaturedProducts?count=5" -Description "GET /api/Product/GetFeaturedProducts"

# Test 5: Get All Products
Write-Host "5?? Testing: Get All Products" -ForegroundColor Cyan
$filterBody = @{
    PageSize = 10
    Page = 1
}
Test-EndpointForSlug -Method "POST" -Url "$baseUrl/api/Product/search" -Description "POST /api/Product/search" -Body $filterBody

# Test 6: Get Products by Category
Write-Host "6?? Testing: Get Products by Category" -ForegroundColor Cyan
$categoryId = "your-category-guid-here"
Test-EndpointForSlug -Method "POST" -Url "$baseUrl/api/Product/category/$categoryId" -Description "POST /api/Product/category/{id}"

# Test 7: Get Products by Subcategory
Write-Host "7?? Testing: Get Products by Subcategory" -ForegroundColor Cyan
$subCategoryId = "your-subcategory-guid-here"
Test-EndpointForSlug -Method "POST" -Url "$baseUrl/api/Product/subcategory/$subCategoryId" -Description "POST /api/Product/subcategory/{id}"

# Test 8: Get All Products (Legacy)
Write-Host "8?? Testing: Get All Products Legacy" -ForegroundColor Cyan
Test-EndpointForSlug -Method "GET" -Url "$baseUrl/api/Product/GetAllProducts" -Description "GET /api/Product/GetAllProducts"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Testing Complete!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Summary
Write-Host ""
Write-Host "?? Summary:" -ForegroundColor Yellow
Write-Host "- All endpoints should return 'slug' property" -ForegroundColor White
Write-Host "- Slug format: 'product-name-shortid'" -ForegroundColor White
Write-Host "- If any tests fail, check:" -ForegroundColor White
Write-Host "  1. Database has Slug column" -ForegroundColor Gray
Write-Host "  2. Products have generated slugs" -ForegroundColor Gray
Write-Host "  3. AutoMapper is configured correctly" -ForegroundColor Gray
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Run database migration: Migrations/20260215_Add_SlugRedirects_Table.sql" -ForegroundColor White
Write-Host "2. Generate slugs for existing products (see SQL script)" -ForegroundColor White
Write-Host "3. Restart API and run this test again" -ForegroundColor White
Write-Host ""
