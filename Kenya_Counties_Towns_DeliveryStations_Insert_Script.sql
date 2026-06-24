-- PostgreSQL Insert Scripts for Kenya Counties, Towns, and Delivery Stations
-- 5 Major Counties in Kenya with 5 Major Towns each + Quickcrate Express Delivery Station

-- ============================================
-- COUNTIES INSERT SCRIPT
-- ============================================

INSERT INTO "Counties" ("CountyCode", "CountyName", "CreatedOn") VALUES
(47, 'Nairobi', NOW()),
(1, 'Mombasa', NOW()),
(32, 'Nakuru', NOW()),
(39, 'Kisumu', NOW()),
(45, 'Eldoret (Uasin Gishu)', NOW())
ON CONFLICT ("CountyCode") DO NOTHING;

-- ============================================
-- TOWNS INSERT SCRIPT
-- ============================================

-- Nairobi County Towns (County Code: 47)
INSERT INTO "Towns" ("TownName", "CountyId", "CreatedOn") VALUES
('Nairobi CBD', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 47), NOW()),
('Westlands', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 47), NOW()),
('Karen', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 47), NOW()),
('Eastlands (Umoja)', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 47), NOW()),
('Kasarani', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 47), NOW())
ON CONFLICT DO NOTHING;

-- Mombasa County Towns (County Code: 1)
INSERT INTO "Towns" ("TownName", "CountyId", "CreatedOn") VALUES
('Mombasa Island', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 1), NOW()),
('Nyali', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 1), NOW()),
('Likoni', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 1), NOW()),
('Changamwe', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 1), NOW()),
('Kisauni', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 1), NOW())
ON CONFLICT DO NOTHING;

-- Nakuru County Towns (County Code: 32)
INSERT INTO "Towns" ("TownName", "CountyId", "CreatedOn") VALUES
('Nakuru Town', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 32), NOW()),
('Naivasha', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 32), NOW()),
('Gilgil', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 32), NOW()),
('Molo', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 32), NOW()),
('Njoro', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 32), NOW())
ON CONFLICT DO NOTHING;

-- Kisumu County Towns (County Code: 39)
INSERT INTO "Towns" ("TownName", "CountyId", "CreatedOn") VALUES
('Kisumu City', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 39), NOW()),
('Maseno', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 39), NOW()),
('Kondele', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 39), NOW()),
('Ahero', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 39), NOW()),
('Muhoroni', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 39), NOW())
ON CONFLICT DO NOTHING;

-- Eldoret (Uasin Gishu) County Towns (County Code: 45)
INSERT INTO "Towns" ("TownName", "CountyId", "CreatedOn") VALUES
('Eldoret Town', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 45), NOW()),
('Kapsaret', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 45), NOW()),
('Moiben', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 45), NOW()),
('Turbo', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 45), NOW()),
('Soy', (SELECT "CountyId" FROM "Counties" WHERE "CountyCode" = 45), NOW())
ON CONFLICT DO NOTHING;

-- ============================================
-- DELIVERY STATIONS INSERT SCRIPT
-- ============================================

-- Quickcrate Express Delivery Station in Nairobi CBD, Moi Avenue
INSERT INTO "DeliveryStations" ("DeliveryStationName", "TownId", "CreatedOn") VALUES
('Quickcrate Express', (SELECT "TownId" FROM "Towns" WHERE "TownName" = 'Nairobi CBD'), NOW())
ON CONFLICT DO NOTHING;

-- ============================================
-- VERIFICATION QUERIES
-- ============================================

-- Verify Counties
SELECT 
    "CountyId",
    "CountyCode", 
    "CountyName", 
    "CreatedOn"::DATE as "DateCreated"
FROM "Counties" 
ORDER BY "CountyCode";

-- Verify Towns by County
SELECT 
    c."CountyName",
    t."TownId",
    t."TownName",
    t."CreatedOn"::DATE as "DateCreated"
FROM "Towns" t
JOIN "Counties" c ON t."CountyId" = c."CountyId"
ORDER BY c."CountyCode", t."TownName";

-- Verify Delivery Station
SELECT 
    ds."DeliveryStationId",
    ds."DeliveryStationName",
    t."TownName",
    c."CountyName",
    ds."CreatedOn"::DATE as "DateCreated"
FROM "DeliveryStations" ds
JOIN "Towns" t ON ds."TownId" = t."TownId"
JOIN "Counties" c ON t."CountyId" = c."CountyId"
ORDER BY ds."DeliveryStationName";

-- Summary Count
SELECT 
    'Counties' as "EntityType", 
    COUNT(*) as "TotalCount" 
FROM "Counties"
UNION ALL
SELECT 
    'Towns' as "EntityType", 
    COUNT(*) as "TotalCount" 
FROM "Towns"
UNION ALL
SELECT 
    'Delivery Stations' as "EntityType", 
    COUNT(*) as "TotalCount" 
FROM "DeliveryStations";

-- ============================================
-- ADDITIONAL NOTES
-- ============================================

/*
COUNTIES INCLUDED:
1. Nairobi (County Code: 47) - Capital and largest city
2. Mombasa (County Code: 1) - Major coastal city and port
3. Nakuru (County Code: 32) - Major agricultural and industrial center
4. Kisumu (County Code: 39) - Major lakeside city, western Kenya hub
5. Eldoret/Uasin Gishu (County Code: 45) - Major highland city, agricultural hub

TOWNS PER COUNTY:
Each county includes 5 major towns/areas that represent significant commercial or residential centers.

DELIVERY STATION:
- Quickcrate Express located in Nairobi CBD
- Positioned on Moi Avenue (implied in name/location)
- Ready for logistics and delivery operations

DATABASE COMPATIBILITY:
- Uses PostgreSQL syntax with double quotes for case-sensitive identifiers
- Includes ON CONFLICT DO NOTHING to prevent duplicate entries
- Uses subqueries for foreign key relationships
- Compatible with your existing entity model structure
*/