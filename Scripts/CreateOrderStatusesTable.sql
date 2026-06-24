-- Create OrderStatuses table if it doesn't exist
CREATE TABLE IF NOT EXISTS "OrderStatuses" (
    "StatusID" SERIAL PRIMARY KEY,
    "Status" VARCHAR(100) NOT NULL,
    "Description" VARCHAR(500) NOT NULL,
    "OrderID" VARCHAR(50),
    "CreatedBy" VARCHAR(100) NOT NULL,
    "CreatedOn" TIMESTAMP NOT NULL,
    "UpdatedBy" VARCHAR(100) NOT NULL,
    "UpdatedOn" TIMESTAMP NOT NULL
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS "IX_OrderStatuses_OrderID" ON "OrderStatuses" ("OrderID");

-- Insert default order statuses if they don't exist
INSERT INTO "OrderStatuses" ("Status", "Description", "CreatedBy", "CreatedOn", "UpdatedBy", "UpdatedOn")
VALUES 
    ('Pending', 'Order is pending processing', 'System', NOW(), 'System', NOW()),
    ('Processing', 'Order is being processed', 'System', NOW(), 'System', NOW()),
    ('Paid', 'Order payment confirmed', 'System', NOW(), 'System', NOW()),
    ('Shipped', 'Order has been shipped', 'System', NOW(), 'System', NOW()),
    ('Delivered', 'Order has been delivered', 'System', NOW(), 'System', NOW()),
    ('Cancelled', 'Order has been cancelled', 'System', NOW(), 'System', NOW()),
    ('Returned', 'Order has been returned', 'System', NOW(), 'System', NOW())
ON CONFLICT DO NOTHING;