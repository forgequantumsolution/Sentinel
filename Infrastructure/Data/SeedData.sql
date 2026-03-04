-- Seed Default Roles
INSERT INTO "Roles" ("Id", "Name", "Description", "IsDefault", "IsActive", "IsDeleted", "CreatedAt")
VALUES 
    (gen_random_uuid(), 'sys-admin', 'System Administrator - Full system access', false, true, false, NOW()),
    (gen_random_uuid(), 'admin', 'Administrator - Full access', false, true, false, NOW()),
    (gen_random_uuid(), 'user', 'Standard User - Basic access', true, true, false, NOW())
ON CONFLICT ("Name") DO NOTHING;

-- Seed Default Admin User
-- Note: 'Approved' status is 1. The PasswordHash is for 'Admin123!'
INSERT INTO "Users" ("Id", "Name", "Email", "PasswordHash", "RoleId", "Status", "CreatedAt", "IsActive", "IsDeleted")
SELECT 
    gen_random_uuid(), 
    'Administrator', 
    'admin@analytics.com', 
    'AQAAAAIAAYagAAAAEJrO6yvXm5H9p0V1Z2W3X4Y5Z6A7B8C9D0E1F2G3H4I5J6K7L8M9N0O1P2Q3R==', 
    "Id", 
    1, 
    NOW(), 
    true, 
    false
FROM "Roles" 
WHERE "Name" = 'admin'
ON CONFLICT ("Email") DO NOTHING;
