-- PostgreSQL Script to Check Missing MerchantID Columns
-- Run this script first to see which columns are missing

SELECT 
    'Products' as table_name,
    CASE 
        WHEN EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_name = 'Products' AND column_name = 'MerchantID'
        ) THEN 'EXISTS' 
        ELSE 'MISSING' 
    END as merchantid_column_status
    
UNION ALL

SELECT 
    'Categories' as table_name,
    CASE 
        WHEN EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_name = 'Categories' AND column_name = 'MerchantID'
        ) THEN 'EXISTS' 
        ELSE 'MISSING' 
    END as merchantid_column_status
    
UNION ALL

SELECT 
    'OrderProducts' as table_name,
    CASE 
        WHEN EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_name = 'OrderProducts' AND column_name = 'MerchantID'
        ) THEN 'EXISTS' 
        ELSE 'MISSING' 
    END as merchantid_column_status
    
UNION ALL

SELECT 
    'OrderTracking' as table_name,
    CASE 
        WHEN EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_name = 'OrderTracking' AND column_name = 'MerchantID'
        ) THEN 'EXISTS' 
        ELSE 'MISSING' 
    END as merchantid_column_status;

-- Also check if tables exist
SELECT 
    table_name,
    'TABLE' as object_type
FROM information_schema.tables 
WHERE table_name IN ('Products', 'Categories', 'OrderProducts', 'OrderTracking', 'Merchants')
ORDER BY table_name;