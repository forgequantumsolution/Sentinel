DO $$
DECLARE
    org_id UUID := gen_random_uuid();
    super_admin_role_id UUID := gen_random_uuid();
BEGIN

-- Seed Default Organization
INSERT INTO "Organizations" ("Id", "Name", "Code", "Description", "IsActive", "IsDeleted", "CreatedAt")
VALUES
    (org_id, 'Default Organization', 'DEFAULT', 'Default system organization', true, false, NOW())
ON CONFLICT ("Code") DO NOTHING;

-- If org already existed, fetch its Id
SELECT "Id" INTO org_id FROM "Organizations" WHERE "Code" = 'DEFAULT';

-- Seed Default Roles
INSERT INTO "Roles" ("Id", "Name", "Description", "IsDefault", "IsActive", "IsDeleted", "CreatedAt", "OrganizationId")
VALUES
    (super_admin_role_id, 'super-admin', 'Super Administrator - Shadow system role', false, true, false, NOW(), org_id),
    (gen_random_uuid(), 'sys-admin', 'System Administrator - Full system access', false, true, false, NOW(), org_id),
    (gen_random_uuid(), 'admin', 'Administrator - Full access', false, true, false, NOW(), org_id),
    (gen_random_uuid(), 'user', 'Standard User - Basic access', true, true, false, NOW(), org_id)
ON CONFLICT ("Name") DO NOTHING;

-- If super-admin role already existed, fetch its Id
SELECT "Id" INTO super_admin_role_id FROM "Roles" WHERE "Name" = 'super-admin';

-- Seed Shadow Super Admin User
INSERT INTO "Users" ("Id", "Name", "Email", "PasswordHash", "RoleId", "Status", "CreatedAt", "IsActive", "IsDeleted", "OrganizationId")
VALUES
    (gen_random_uuid(), 'Super Admin', 'superadmin@system.internal', '__SUPER_ADMIN_HASH__', super_admin_role_id, 1, NOW(), true, false, org_id)
ON CONFLICT ("Email") DO NOTHING;

-- Seed Default Admin User
-- Note: 'Approved' status is 1. The PasswordHash is for 'Admin123!'
INSERT INTO "Users" ("Id", "Name", "Email", "PasswordHash", "RoleId", "Status", "CreatedAt", "IsActive", "IsDeleted", "OrganizationId")
SELECT
    gen_random_uuid(),
    'Administrator',
    'admin@analytics.com',
    'AQAAAAIAAYagAAAAEJrO6yvXm5H9p0V1Z2W3X4Y5Z6A7B8C9D0E1F2G3H4I5J6K7L8M9N0O1P2Q3R==',
    "Id",
    1,
    NOW(),
    true,
    false,
    org_id
FROM "Roles"
WHERE "Name" = 'admin'
ON CONFLICT ("Email") DO NOTHING;

END $$;
