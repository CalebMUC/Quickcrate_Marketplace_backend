-- Create OrderStatuses table if it doesn't exist and insert default statuses
-- This script is safe to run multiple times and preserves existing data

DO $$
BEGIN
    -- Check if OrderStatuses table exists, if not create it
    IF NOT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name = 'OrderStatuses'
    ) THEN
        CREATE TABLE "OrderStatuses" (
            "StatusID" SERIAL PRIMARY KEY,
            "Status" character varying(100) NOT NULL,
            "Description" character varying(500) NOT NULL,
            "CreatedBy" character varying(100) NOT NULL,
            "CreatedOn" timestamp NOT NULL DEFAULT now(),
            "UpdatedBy" character varying(100) NOT NULL,
            "UpdatedOn" timestamp NOT NULL DEFAULT now(),
            "OrderID" character varying(50) NULL
        );
        
        CREATE INDEX IF NOT EXISTS "IX_OrderStatuses_OrderID" ON "OrderStatuses" ("OrderID");
        
        -- Insert default order statuses
        INSERT INTO "OrderStatuses" ("Status", "Description", "CreatedBy", "UpdatedBy") VALUES
        ('Pending', 'Order has been created and is waiting for payment', 'System', 'System'),
        ('Processing', 'Order is being processed', 'System', 'System'),
        ('Shipped', 'Order has been shipped', 'System', 'System'),
        ('Delivered', 'Order has been delivered', 'System', 'System'),
        ('Cancelled', 'Order has been cancelled', 'System', 'System'),
        ('Refunded', 'Order has been refunded', 'System', 'System');
        
        RAISE NOTICE 'OrderStatuses table created and populated with default data';
    ELSE
        -- Table exists, ensure we have basic statuses
        INSERT INTO "OrderStatuses" ("Status", "Description", "CreatedBy", "UpdatedBy")
        SELECT 'Pending', 'Order has been created and is waiting for payment', 'System', 'System'
        WHERE NOT EXISTS (SELECT 1 FROM "OrderStatuses" WHERE "Status" = 'Pending');
        
        INSERT INTO "OrderStatuses" ("Status", "Description", "CreatedBy", "UpdatedBy")
        SELECT 'Processing', 'Order is being processed', 'System', 'System'
        WHERE NOT EXISTS (SELECT 1 FROM "OrderStatuses" WHERE "Status" = 'Processing');
        
        INSERT INTO "OrderStatuses" ("Status", "Description", "CreatedBy", "UpdatedBy")
        SELECT 'Shipped', 'Order has been shipped', 'System', 'System'
        WHERE NOT EXISTS (SELECT 1 FROM "OrderStatuses" WHERE "Status" = 'Shipped');
        
        INSERT INTO "OrderStatuses" ("Status", "Description", "CreatedBy", "UpdatedBy")
        SELECT 'Delivered', 'Order has been delivered', 'System', 'System'
        WHERE NOT EXISTS (SELECT 1 FROM "OrderStatuses" WHERE "Status" = 'Delivered');
        
        INSERT INTO "OrderStatuses" ("Status", "Description", "CreatedBy", "UpdatedBy")
        SELECT 'Cancelled', 'Order has been cancelled', 'System', 'System'
        WHERE NOT EXISTS (SELECT 1 FROM "OrderStatuses" WHERE "Status" = 'Cancelled');
        
        INSERT INTO "OrderStatuses" ("Status", "Description", "CreatedBy", "UpdatedBy")
        SELECT 'Refunded', 'Order has been refunded', 'System', 'System'
        WHERE NOT EXISTS (SELECT 1 FROM "OrderStatuses" WHERE "Status" = 'Refunded');
        
        RAISE NOTICE 'OrderStatuses table already exists, missing statuses added';
    END IF;
END
$$;