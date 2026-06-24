-- Migration Script: Remove Certificate Fields and Add Documents Array
-- Run this migration script to update the Merchants table

-- Step 1: Add the new Documents column as text array
ALTER TABLE "Merchants" 
ADD COLUMN "Documents" text[] DEFAULT '{}';

-- Step 2: Migrate existing certificate data to Documents array (if any exists)
-- This will combine KRAPINCertificate and BusinessRegistrationCertificate into the Documents array
UPDATE "Merchants" 
SET "Documents" = array_remove(
    ARRAY[
        CASE WHEN "KRAPINCertificate" IS NOT NULL AND "KRAPINCertificate" != '' 
             THEN "KRAPINCertificate" 
             ELSE NULL 
        END,
        CASE WHEN "BusinessRegistrationCertificate" IS NOT NULL AND "BusinessRegistrationCertificate" != '' 
             THEN "BusinessRegistrationCertificate" 
             ELSE NULL 
        END
    ], 
    NULL
)
WHERE "KRAPINCertificate" IS NOT NULL OR "BusinessRegistrationCertificate" IS NOT NULL;

-- Step 3: Drop the old certificate columns
-- Uncomment these lines after ensuring data migration is successful
-- ALTER TABLE "Merchants" DROP COLUMN IF EXISTS "KRAPINCertificate";
-- ALTER TABLE "Merchants" DROP COLUMN IF EXISTS "BusinessRegistrationCertificate";

-- Optional: Drop other legacy payment fields that are now handled by MerchantPaymentMethods table
-- Uncomment these lines if you want to remove legacy payment fields as well
-- ALTER TABLE "Merchants" DROP COLUMN IF EXISTS "BankName";
-- ALTER TABLE "Merchants" DROP COLUMN IF EXISTS "BankAccountNo";
-- ALTER TABLE "Merchants" DROP COLUMN IF EXISTS "BankAccountName";
-- ALTER TABLE "Merchants" DROP COLUMN IF EXISTS "MpesaPaybill";
-- ALTER TABLE "Merchants" DROP COLUMN IF EXISTS "MpesaTillNumber";
-- ALTER TABLE "Merchants" DROP COLUMN IF EXISTS "PreferredPaymentChannel";

-- Step 4: Add index on Documents column for better search performance (optional)
-- CREATE INDEX IF NOT EXISTS "IX_Merchants_Documents" ON "Merchants" USING GIN ("Documents");