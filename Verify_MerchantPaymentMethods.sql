-- Verification and Sample Data Script for MerchantPaymentMethods

-- 1. Check if the table exists
SELECT EXISTS (
    SELECT FROM information_schema.tables 
    WHERE table_name = 'MerchantPaymentMethods'
) AS table_exists;

-- 2. Check table structure
SELECT 
    column_name, 
    data_type, 
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'MerchantPaymentMethods' 
ORDER BY ordinal_position;

-- 3. Check foreign key constraints
SELECT
    tc.table_name,
    kcu.column_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY'
    AND tc.table_name = 'MerchantPaymentMethods';

-- 4. Insert sample payment methods if they don't exist
INSERT INTO "PaymentMethods" ("Name", "Description", "IsActive", "ImageUrl", "CreatedDate", "UpdatedOn") 
VALUES 
    ('M-Pesa', 'Mobile money payment via Safaricom M-Pesa', true, '/images/mpesa.png', NOW(), NOW()),
    ('Bank Transfer', 'Direct bank transfer payment', true, '/images/bank.png', NOW(), NOW()),
    ('Cash on Delivery', 'Pay cash when you receive your order', true, '/images/cod.png', NOW(), NOW()),
    ('Credit Card', 'Visa/Mastercard payment', true, '/images/card.png', NOW(), NOW()),
    ('PayPal', 'PayPal online payment', true, '/images/paypal.png', NOW(), NOW())
ON CONFLICT ("Name") DO NOTHING;

-- 5. Check existing payment methods
SELECT * FROM "PaymentMethods" ORDER BY "PaymentMethodID";

-- 6. Sample merchant payment method configuration (uncomment and modify as needed)
/*
-- Example: Add M-Pesa configuration for a merchant
INSERT INTO "MerchantPaymentMethods" ("MerchantId", "PaymentMethodId", "Configuration", "IsEnabled", "CreatedAt")
VALUES (
    'your-merchant-guid-here', -- Replace with actual merchant ID
    1, -- M-Pesa payment method ID
    '{"accountNumber":"123456","accountName":"Paybill","phoneNumber":"0714262062","merchantCode":"99890","additionalInfo":"Mpesa"}',
    true,
    NOW()
);

-- Example: Add Bank Transfer configuration for a merchant
INSERT INTO "MerchantPaymentMethods" ("MerchantId", "PaymentMethodId", "Configuration", "IsEnabled", "CreatedAt")
VALUES (
    'your-merchant-guid-here', -- Replace with actual merchant ID
    2, -- Bank Transfer payment method ID
    '{"accountNumber":"000019903802","accountName":"Victor Auto Spares","phoneNumber":"0714262062","merchantCode":"008090","additionalInfo":"Bank Details"}',
    true,
    NOW()
);
*/

-- 7. Check merchant payment methods (will be empty until you add some)
SELECT 
    mpm."Id",
    mpm."MerchantId",
    pm."Name" AS "PaymentMethodName",
    mpm."Configuration",
    mpm."IsEnabled",
    mpm."CreatedAt"
FROM "MerchantPaymentMethods" mpm
JOIN "PaymentMethods" pm ON mpm."PaymentMethodId" = pm."PaymentMethodID"
ORDER BY mpm."CreatedAt" DESC;