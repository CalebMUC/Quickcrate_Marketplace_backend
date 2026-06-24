-- Fix Foreign Key Naming Consistency Migration Script
-- This script helps ensure all foreign keys use consistent naming (ApplicationUserId)

-- 1. Update RefreshTokens table to use ApplicationUserId
ALTER TABLE "RefreshTokens" 
RENAME COLUMN "UserId" TO "ApplicationUserId";

-- 2. Update Merchants table to use ApplicationUserId  
ALTER TABLE "Merchants"
RENAME COLUMN "UserId" TO "ApplicationUserId";

-- 3. Create indexes for better performance (if they don't exist)
CREATE INDEX IF NOT EXISTS "IX_RefreshTokens_ApplicationUserId" ON "RefreshTokens" ("ApplicationUserId");
CREATE INDEX IF NOT EXISTS "IX_Merchants_ApplicationUserId" ON "Merchants" ("ApplicationUserId");

-- 4. Update foreign key constraints to reference the correct column names
-- Note: You may need to drop and recreate foreign key constraints if they exist

-- Drop existing foreign key constraints (if they exist)
-- ALTER TABLE "RefreshTokens" DROP CONSTRAINT IF EXISTS "FK_RefreshTokens_AspNetUsers_UserId";
-- ALTER TABLE "Merchants" DROP CONSTRAINT IF EXISTS "FK_Merchants_AspNetUsers_UserId";

-- Add corrected foreign key constraints
-- ALTER TABLE "RefreshTokens" 
-- ADD CONSTRAINT "FK_RefreshTokens_AspNetUsers_ApplicationUserId" 
-- FOREIGN KEY ("ApplicationUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE;

-- ALTER TABLE "Merchants" 
-- ADD CONSTRAINT "FK_Merchants_AspNetUsers_ApplicationUserId" 
-- FOREIGN KEY ("ApplicationUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE SET NULL;

-- 5. Verification queries to ensure consistency
SELECT 
    'RefreshTokens' as TableName,
    COUNT(*) as TotalRecords,
    COUNT("ApplicationUserId") as RecordsWithApplicationUserId
FROM "RefreshTokens"
UNION ALL
SELECT 
    'Merchants' as TableName,
    COUNT(*) as TotalRecords,
    COUNT("ApplicationUserId") as RecordsWithApplicationUserId
FROM "Merchants"
UNION ALL
SELECT 
    'Orders' as TableName,
    COUNT(*) as TotalRecords,
    COUNT("ApplicationUserId") as RecordsWithApplicationUserId
FROM "Orders"
UNION ALL
SELECT 
    'Cart' as TableName,
    COUNT(*) as TotalRecords,
    COUNT("ApplicationUserId") as RecordsWithApplicationUserId
FROM "Cart"
UNION ALL
SELECT 
    'Reviews' as TableName,
    COUNT(*) as TotalRecords,
    COUNT("ApplicationUserId") as RecordsWithApplicationUserId
FROM "Reviews"
UNION ALL
SELECT 
    'SavedItems' as TableName,
    COUNT(*) as TotalRecords,
    COUNT("ApplicationUserId") as RecordsWithApplicationUserId
FROM "SavedItems"
UNION ALL
SELECT 
    'Addresses' as TableName,
    COUNT(*) as TotalRecords,
    COUNT("ApplicationUserId") as RecordsWithApplicationUserId
FROM "Addresses";