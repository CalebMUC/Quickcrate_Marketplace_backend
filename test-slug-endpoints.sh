#!/bin/bash
# Test All Product Endpoints for Slug Property
# Run this in bash/Linux terminal

BASE_URL="http://localhost:5000"

echo "========================================"
echo "Testing All Product Endpoints for Slug"
echo "========================================"
echo ""

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
GRAY='\033[0;37m'
NC='\033[0m' # No Color

# Function to test endpoint
test_endpoint() {
    local method=$1
    local url=$2
    local description=$3
    local body=$4
    
    echo -e "${YELLOW}Testing: $description${NC}"
    echo -e "${GRAY}URL: $method $url${NC}"
    
    if [ "$method" == "POST" ]; then
        response=$(curl -s -X POST "$url" \
            -H "Content-Type: application/json" \
            -d "$body")
    else
        response=$(curl -s -X GET "$url")
    fi
    
    # Check if response contains slug
    if echo "$response" | grep -q '"slug"'; then
        slug=$(echo "$response" | grep -o '"slug":"[^"]*"' | head -1 | cut -d'"' -f4)
        echo -e "${GREEN}? PASS - Slug found: $slug${NC}"
    else
        echo -e "${RED}? FAIL - No slug property found in response${NC}"
        echo -e "${GRAY}Response:${NC}"
        echo "$response" | jq '.' 2>/dev/null || echo "$response"
    fi
    
    echo ""
}

# Test 1: Get Product by ID
echo -e "${CYAN}1?? Testing: Get Product by ID${NC}"
PRODUCT_ID="6949aa56-xxxx-xxxx-xxxx-xxxxxxxxxxxx"  # Replace with real ID
test_endpoint "GET" "$BASE_URL/api/Product/$PRODUCT_ID" "GET /api/Product/{id}"

# Test 2: Get Product Enhanced
echo -e "${CYAN}2?? Testing: Get Product Enhanced${NC}"
test_endpoint "GET" "$BASE_URL/api/Product/GetProduct/$PRODUCT_ID" "GET /api/Product/GetProduct/{id}"

# Test 3: Get Product by Slug
echo -e "${CYAN}3?? Testing: Get Product by Slug${NC}"
test_endpoint "GET" "$BASE_URL/api/Product/slug/getac-k120-core-i5-6949aa56" "GET /api/Product/slug/{slug}"

# Test 4: Get Featured Products
echo -e "${CYAN}4?? Testing: Get Featured Products${NC}"
test_endpoint "GET" "$BASE_URL/api/Product/GetFeaturedProducts?count=5" "GET /api/Product/GetFeaturedProducts"

# Test 5: Get All Products
echo -e "${CYAN}5?? Testing: Get All Products${NC}"
FILTER_BODY='{"PageSize":10,"Page":1}'
test_endpoint "POST" "$BASE_URL/api/Product/search" "POST /api/Product/search" "$FILTER_BODY"

# Test 6: Get Products by Category
echo -e "${CYAN}6?? Testing: Get Products by Category${NC}"
CATEGORY_ID="your-category-guid-here"  # Replace with real category ID
test_endpoint "POST" "$BASE_URL/api/Product/category/$CATEGORY_ID" "POST /api/Product/category/{id}"

# Test 7: Get Products by Subcategory
echo -e "${CYAN}7?? Testing: Get Products by Subcategory${NC}"
SUBCATEGORY_ID="your-subcategory-guid-here"  # Replace with real subcategory ID
test_endpoint "POST" "$BASE_URL/api/Product/subcategory/$SUBCATEGORY_ID" "POST /api/Product/subcategory/{id}"

# Test 8: Get All Products (Legacy)
echo -e "${CYAN}8?? Testing: Get All Products Legacy${NC}"
test_endpoint "GET" "$BASE_URL/api/Product/GetAllProducts" "GET /api/Product/GetAllProducts"

echo "========================================"
echo "Testing Complete!"
echo "========================================"

# Summary
echo ""
echo -e "${YELLOW}?? Summary:${NC}"
echo "- All endpoints should return 'slug' property"
echo "- Slug format: 'product-name-shortid'"
echo "- If any tests fail, check:"
echo "  1. Database has Slug column"
echo "  2. Products have generated slugs"
echo "  3. AutoMapper is configured correctly"
echo ""
echo -e "${YELLOW}Next Steps:${NC}"
echo "1. Run database migration: Migrations/20260215_Add_SlugRedirects_Table.sql"
echo "2. Generate slugs for existing products (see SQL script)"
echo "3. Restart API and run this test again"
echo ""
