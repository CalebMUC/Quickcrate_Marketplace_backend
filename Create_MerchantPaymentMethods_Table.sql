-- Create MerchantPaymentMethods table
-- This script creates the missing MerchantPaymentMethods table that's referenced in the code

-- Create the MerchantPaymentMethods table
CREATE TABLE IF NOT EXISTS "MerchantPaymentMethods" (
    "Id" SERIAL PRIMARY KEY,
    "MerchantId" uuid NOT NULL,
    "PaymentMethodId" integer NOT NULL,
    "Configuration" character varying(500),
    "IsEnabled" boolean NOT NULL DEFAULT true,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    "UpdatedAt" timestamp with time zone,
    
    -- Foreign key constraints
    CONSTRAINT "FK_MerchantPaymentMethods_Merchants_MerchantId" 
        FOREIGN KEY ("MerchantId") REFERENCES "Merchants"("MerchantID") ON DELETE CASCADE,
    CONSTRAINT "FK_MerchantPaymentMethods_PaymentMethods_PaymentMethodId" 
        FOREIGN KEY ("PaymentMethodId") REFERENCES "PaymentMethods"("PaymentMethodID") ON DELETE RESTRICT,
    
    -- Unique constraint to prevent duplicate payment methods per merchant
    CONSTRAINT "UQ_MerchantPaymentMethods_MerchantId_PaymentMethodId" 
        UNIQUE ("MerchantId", "PaymentMethodId")
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS "IX_MerchantPaymentMethods_MerchantId" 
    ON "MerchantPaymentMethods" ("MerchantId");

CREATE INDEX IF NOT EXISTS "IX_MerchantPaymentMethods_PaymentMethodId" 
    ON "MerchantPaymentMethods" ("PaymentMethodId");

CREATE INDEX IF NOT EXISTS "IX_MerchantPaymentMethods_IsEnabled" 
    ON "MerchantPaymentMethods" ("IsEnabled");

-- Add some sample data if needed (optional - uncomment if you want sample data)
/*
-- Insert sample payment methods if they don't exist
INSERT INTO "PaymentMethods" ("Name", "Description", "IsActive", "ImageUrl", "CreatedDate") 
VALUES 
    ('M-Pesa', 'Mobile money payment via Safaricom M-Pesa', true, '/images/mpesa.png', NOW()),
    ('Bank Transfer', 'Direct bank transfer payment', true, '/images/bank.png', NOW()),
    ('Cash on Delivery', 'Pay cash when you receive your order', true, '/images/cod.png', NOW())
ON CONFLICT ("Name") DO NOTHING;
*/

-- Verify the table was created successfully
SELECT 
    table_name, 
    column_name, 
    data_type, 
    is_nullable
FROM information_schema.columns 
WHERE table_name = 'MerchantPaymentMethods' 
ORDER BY ordinal_position;