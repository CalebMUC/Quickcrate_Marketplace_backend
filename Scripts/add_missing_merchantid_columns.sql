-- PostgreSQL Script to Add Missing MerchantID Columns
-- Run this script in your PostgreSQL database to add the missing columns

BEGIN;

-- ==========================================
-- STEP 1: Add MerchantID to Products table
-- ==========================================

DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'Products' AND column_name = 'MerchantID'
    ) THEN
        ALTER TABLE "Products" ADD COLUMN "MerchantID" UUID NULL;
        RAISE NOTICE 'Added MerchantID column to Products table';
    ELSE
        RAISE NOTICE 'MerchantID column already exists in Products table';
    END IF;
END $$;

-- ==========================================
-- STEP 2: Add MerchantID to Categories table
-- ==========================================

DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'Categories' AND column_name = 'MerchantID'
    ) THEN
        ALTER TABLE "Categories" ADD COLUMN "MerchantID" UUID NULL;
        RAISE NOTICE 'Added MerchantID column to Categories table';
    ELSE
        RAISE NOTICE 'MerchantID column already exists in Categories table';
    END IF;
END $$;

-- ==========================================
-- STEP 3: Add MerchantID to OrderProducts table
-- ==========================================

DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'OrderProducts' AND column_name = 'MerchantID'
    ) THEN
        ALTER TABLE "OrderProducts" ADD COLUMN "MerchantID" UUID NULL;
        RAISE NOTICE 'Added MerchantID column to OrderProducts table';
    ELSE
        RAISE NOTICE 'MerchantID column already exists in OrderProducts table';
    END IF;
END $$;

-- ==========================================
-- STEP 4: Add MerchantID to OrderTracking table
-- ==========================================

DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'OrderTracking' AND column_name = 'MerchantID'
    ) THEN
        ALTER TABLE "OrderTracking" ADD COLUMN "MerchantID" UUID NULL;
        RAISE NOTICE 'Added MerchantID column to OrderTracking table';
    ELSE
        RAISE NOTICE 'MerchantID column already exists in OrderTracking table';
    END IF;
END $$;

-- ==========================================
-- STEP 5: Create Foreign Key Constraints
-- ==========================================

-- Foreign key for Products -> Merchants
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_Products_Merchants_MerchantID'
    ) THEN
        ALTER TABLE "Products"
        ADD CONSTRAINT "FK_Products_Merchants_MerchantID"
        FOREIGN KEY ("MerchantID") 
        REFERENCES "Merchants"("MerchantID") 
        ON DELETE SET NULL;
        RAISE NOTICE 'Added foreign key constraint for Products -> Merchants';
    ELSE
        RAISE NOTICE 'Foreign key constraint already exists for Products -> Merchants';
    END IF;
END $$;

-- Foreign key for Categories -> Merchants
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_Categories_Merchants_MerchantID'
    ) THEN
        ALTER TABLE "Categories"
        ADD CONSTRAINT "FK_Categories_Merchants_MerchantID"
        FOREIGN KEY ("MerchantID") 
        REFERENCES "Merchants"("MerchantID") 
        ON DELETE SET NULL;
        RAISE NOTICE 'Added foreign key constraint for Categories -> Merchants';
    ELSE
        RAISE NOTICE 'Foreign key constraint already exists for Categories -> Merchants';
    END IF;
END $$;

-- Foreign key for OrderProducts -> Merchants
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_OrderProducts_Merchants_MerchantID'
    ) THEN
        ALTER TABLE "OrderProducts"
        ADD CONSTRAINT "FK_OrderProducts_Merchants_MerchantID"
        FOREIGN KEY ("MerchantID") 
        REFERENCES "Merchants"("MerchantID") 
        ON DELETE SET NULL;
        RAISE NOTICE 'Added foreign key constraint for OrderProducts -> Merchants';
    ELSE
        RAISE NOTICE 'Foreign key constraint already exists for OrderProducts -> Merchants';
    END IF;
END $$;

-- Foreign key for OrderTracking -> Merchants
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_OrderTracking_Merchants_MerchantID'
    ) THEN
        ALTER TABLE "OrderTracking"
        ADD CONSTRAINT "FK_OrderTracking_Merchants_MerchantID"
        FOREIGN KEY ("MerchantID") 
        REFERENCES "Merchants"("MerchantID") 
        ON DELETE SET NULL;
        RAISE NOTICE 'Added foreign key constraint for OrderTracking -> Merchants';
    ELSE
        RAISE NOTICE 'Foreign key constraint already exists for OrderTracking -> Merchants';
    END IF;
END $$;

-- ==========================================
-- STEP 6: Create Performance Indexes
-- ==========================================

-- Index on Products.MerchantID
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes 
        WHERE indexname = 'IX_Products_MerchantID'
    ) THEN
        CREATE INDEX "IX_Products_MerchantID" 
        ON "Products" ("MerchantID");
        RAISE NOTICE 'Created index IX_Products_MerchantID';
    ELSE
        RAISE NOTICE 'Index IX_Products_MerchantID already exists';
    END IF;
END $$;

-- Index on Categories.MerchantID
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes 
        WHERE indexname = 'IX_Categories_MerchantID'
    ) THEN
        CREATE INDEX "IX_Categories_MerchantID"
        ON "Categories" ("MerchantID");
        RAISE NOTICE 'Created index IX_Categories_MerchantID';
    ELSE
        RAISE NOTICE 'Index IX_Categories_MerchantID already exists';
    END IF;
END $$;

-- Index on OrderProducts.MerchantID
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes 
        WHERE indexname = 'IX_OrderProducts_MerchantID'
    ) THEN
        CREATE INDEX "IX_OrderProducts_MerchantID"
        ON "OrderProducts" ("MerchantID");
        RAISE NOTICE 'Created index IX_OrderProducts_MerchantID';
    ELSE
        RAISE NOTICE 'Index IX_OrderProducts_MerchantID already exists';
    END IF;
END $$;

-- Index on OrderTracking.MerchantID
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes 
        WHERE indexname = 'IX_OrderTracking_MerchantID'
    ) THEN
        CREATE INDEX "IX_OrderTracking_MerchantID" 
        ON "OrderTracking" ("MerchantID");
        RAISE NOTICE 'Created index IX_OrderTracking_MerchantID';
    ELSE
        RAISE NOTICE 'Index IX_OrderTracking_MerchantID already exists';
    END IF;
END $$;

-- ==========================================
-- STEP 7: Update Existing Data (Optional)
-- ==========================================

-- You can add data migration logic here if needed
-- For example, if you have existing products that should be assigned to a default merchant:

/*
-- Example: Assign all existing products to a default merchant (uncomment if needed)
UPDATE "Products" 
SET "MerchantID" = (SELECT "MerchantID" FROM "Merchants" LIMIT 1)
WHERE "MerchantID" IS NULL;

-- Example: Assign all existing categories to a default merchant (uncomment if needed)
UPDATE "Categories" 
SET "MerchantID" = (SELECT "MerchantID" FROM "Merchants" LIMIT 1)
WHERE "MerchantID" IS NULL;
*/

COMMIT;

RAISE NOTICE 'Successfully added all missing MerchantID columns and constraints!';