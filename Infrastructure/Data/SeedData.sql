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
ON CONFLICT ("Name", "OrganizationId") DO NOTHING;

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

-- Seed App Permissions (CRUD + common actions)
INSERT INTO "AppPermissions" ("Id", "Name", "Code", "Description", "IsActive", "IsDeleted", "CreatedAt")
VALUES
    (gen_random_uuid(), 'Create',   'CREATE',   'Create new resources',           true, false, NOW()),
    (gen_random_uuid(), 'Read',     'READ',     'View/read resources',            true, false, NOW()),
    (gen_random_uuid(), 'Update',   'UPDATE',   'Modify existing resources',      true, false, NOW()),
    (gen_random_uuid(), 'Delete',   'DELETE',   'Remove resources',               true, false, NOW())
    -- (gen_random_uuid(), 'Export',   'EXPORT',   'Export data',                    true, false, NOW()),
    -- (gen_random_uuid(), 'Import',   'IMPORT',   'Import data',                    true, false, NOW()),
    -- (gen_random_uuid(), 'Approve',  'APPROVE',  'Approve requests or workflows',  true, false, NOW()),
    -- (gen_random_uuid(), 'Reject',   'REJECT',   'Reject requests or workflows',   true, false, NOW()),
    -- (gen_random_uuid(), 'Assign',   'ASSIGN',   'Assign resources to users',      true, false, NOW()),
    -- (gen_random_uuid(), 'Revoke',   'REVOKE',   'Revoke access or assignments',   true, false, NOW()),
    -- (gen_random_uuid(), 'Execute',  'EXECUTE',  'Run or execute operations',      true, false, NOW()),
    -- (gen_random_uuid(), 'Share',    'SHARE',    'Share resources with others',    true, false, NOW()),
    -- (gen_random_uuid(), 'Download', 'DOWNLOAD', 'Download files or reports',      true, false, NOW()),
    -- (gen_random_uuid(), 'Upload',   'UPLOAD',   'Upload files or data',           true, false, NOW()),
    -- (gen_random_uuid(), 'Manage',   'MANAGE',   'Full management access',         true, false, NOW())
ON CONFLICT ("Code") DO NOTHING;

-- Seed ActionObjects (Features / Navigation)
-- ObjectType enum: Feature=0, Url=1, File=2, DatabaseTable=3, UIComponent=4, Workflow=5, Report=6, Graph=7, Folder=8, Custom=9
DO $$
DECLARE
    -- Top-level sections
    dashboards_id UUID := gen_random_uuid();
    config_id     UUID := gen_random_uuid();
    settings_id   UUID := gen_random_uuid();
BEGIN

-- Parent: Dashboards (NULL route — use NOT EXISTS)
INSERT INTO "ActionObjects" ("Id", "Name", "Code", "Description", "ObjectType", "Route", "SortOrder", "ParentObjectId", "IsActive", "IsDeleted", "CreatedAt")
SELECT dashboards_id, 'Dashboards', 'DASHBOARDS', 'Dashboard section', 8, NULL, 0, NULL, true, false, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "ActionObjects" WHERE "Code" = 'DASHBOARDS');
SELECT "Id" INTO dashboards_id FROM "ActionObjects" WHERE "Code" = 'DASHBOARDS';

-- Dashboards children
INSERT INTO "ActionObjects" ("Id", "Name", "Code", "Description", "ObjectType", "Route", "SortOrder", "ParentObjectId", "IsActive", "IsDeleted", "CreatedAt")
SELECT gen_random_uuid(), 'Insights', 'INSIGHTS', 'Insights dashboard', 0, '/analytics/dashboards', 1, dashboards_id, true, false, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "ActionObjects" WHERE "Code" = 'INSIGHTS');

INSERT INTO "ActionObjects" ("Id", "Name", "Code", "Description", "ObjectType", "Route", "SortOrder", "ParentObjectId", "IsActive", "IsDeleted", "CreatedAt")
SELECT gen_random_uuid(), 'Log Data', 'LOG_DATA', 'Log data viewer', 0, '/analytics/data-entry', 2, dashboards_id, true, false, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "ActionObjects" WHERE "Code" = 'LOG_DATA');

INSERT INTO "ActionObjects" ("Id", "Name", "Code", "Description", "ObjectType", "Route", "SortOrder", "ParentObjectId", "IsActive", "IsDeleted", "CreatedAt")
SELECT gen_random_uuid(), 'Integrations', 'INTEGRATIONS', 'External integrations', 0, '/analytics/integrations', 3, dashboards_id, true, false, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "ActionObjects" WHERE "Code" = 'INTEGRATIONS');

-- Parent: Configuration (NULL route — use NOT EXISTS)
INSERT INTO "ActionObjects" ("Id", "Name", "Code", "Description", "ObjectType", "Route", "SortOrder", "ParentObjectId", "IsActive", "IsDeleted", "CreatedAt")
SELECT config_id, 'Configuration', 'CONFIGURATION', 'Configuration section', 8, NULL, 0, NULL, true, false, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "ActionObjects" WHERE "Code" = 'CONFIGURATION');
SELECT "Id" INTO config_id FROM "ActionObjects" WHERE "Code" = 'CONFIGURATION';

-- Configuration children
INSERT INTO "ActionObjects" ("Id", "Name", "Code", "Description", "ObjectType", "Route", "SortOrder", "ParentObjectId", "IsActive", "IsDeleted", "CreatedAt")
SELECT gen_random_uuid(), 'Form', 'FORM', 'Dynamic forms', 0, '/analytics/ui-configuration', 1, config_id, true, false, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "ActionObjects" WHERE "Code" = 'FORM');

INSERT INTO "ActionObjects" ("Id", "Name", "Code", "Description", "ObjectType", "Route", "SortOrder", "ParentObjectId", "IsActive", "IsDeleted", "CreatedAt")
SELECT gen_random_uuid(), 'Graph Studio', 'GRAPH_STUDIO', 'Graph studio editor', 0, '/analytics/dashboard-config', 2, config_id, true, false, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "ActionObjects" WHERE "Code" = 'GRAPH_STUDIO');

INSERT INTO "ActionObjects" ("Id", "Name", "Code", "Description", "ObjectType", "Route", "SortOrder", "ParentObjectId", "IsActive", "IsDeleted", "CreatedAt")
SELECT gen_random_uuid(), 'Graph Manager', 'GRAPH_MANAGER', 'Graph management', 0, '/analytics/graph-creator', 3, config_id, true, false, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "ActionObjects" WHERE "Code" = 'GRAPH_MANAGER');

INSERT INTO "ActionObjects" ("Id", "Name", "Code", "Description", "ObjectType", "Route", "SortOrder", "ParentObjectId", "IsActive", "IsDeleted", "CreatedAt")
SELECT gen_random_uuid(), 'Data Configuration', 'DATA_CONFIGURATION', 'Data source configuration', 0, '/analytics/data-configuration', 4, config_id, true, false, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "ActionObjects" WHERE "Code" = 'DATA_CONFIGURATION');

INSERT INTO "ActionObjects" ("Id", "Name", "Code", "Description", "ObjectType", "Route", "SortOrder", "ParentObjectId", "IsActive", "IsDeleted", "CreatedAt")
SELECT settings_id, 'Settings', 'SETTINGS', 'Application settings', 0, '/analytics/settings', 5, config_id, true, false, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "ActionObjects" WHERE "Code" = 'SETTINGS');
SELECT "Id" INTO settings_id FROM "ActionObjects" WHERE "Code" = 'SETTINGS';

-- Settings children (NULL routes — insert one by one with NOT EXISTS)
INSERT INTO "ActionObjects" ("Id", "Name", "Code", "Description", "ObjectType", "Route", "SortOrder", "ParentObjectId", "IsActive", "IsDeleted", "CreatedAt")
SELECT gen_random_uuid(), 'Appearance', 'SETTINGS_APPEARANCE', 'Appearance settings', 0, NULL, 1, settings_id, true, false, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "ActionObjects" WHERE "Code" = 'SETTINGS_APPEARANCE');

INSERT INTO "ActionObjects" ("Id", "Name", "Code", "Description", "ObjectType", "Route", "SortOrder", "ParentObjectId", "IsActive", "IsDeleted", "CreatedAt")
SELECT gen_random_uuid(), 'Profile', 'SETTINGS_PROFILE', 'Profile settings', 0, NULL, 2, settings_id, true, false, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "ActionObjects" WHERE "Code" = 'SETTINGS_PROFILE');

INSERT INTO "ActionObjects" ("Id", "Name", "Code", "Description", "ObjectType", "Route", "SortOrder", "ParentObjectId", "IsActive", "IsDeleted", "CreatedAt")
SELECT gen_random_uuid(), 'Users', 'SETTINGS_USERS', 'User management', 0, NULL, 3, settings_id, true, false, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "ActionObjects" WHERE "Code" = 'SETTINGS_USERS');

INSERT INTO "ActionObjects" ("Id", "Name", "Code", "Description", "ObjectType", "Route", "SortOrder", "ParentObjectId", "IsActive", "IsDeleted", "CreatedAt")
SELECT gen_random_uuid(), 'Organization', 'SETTINGS_ORGANIZATION', 'Organization settings', 0, NULL, 4, settings_id, true, false, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "ActionObjects" WHERE "Code" = 'SETTINGS_ORGANIZATION');

INSERT INTO "ActionObjects" ("Id", "Name", "Code", "Description", "ObjectType", "Route", "SortOrder", "ParentObjectId", "IsActive", "IsDeleted", "CreatedAt")
SELECT gen_random_uuid(), 'Departments', 'SETTINGS_DEPARTMENTS', 'Department management', 0, NULL, 5, settings_id, true, false, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "ActionObjects" WHERE "Code" = 'SETTINGS_DEPARTMENTS');

INSERT INTO "ActionObjects" ("Id", "Name", "Code", "Description", "ObjectType", "Route", "SortOrder", "ParentObjectId", "IsActive", "IsDeleted", "CreatedAt")
SELECT gen_random_uuid(), 'Roles', 'SETTINGS_ROLES', 'Role management', 0, NULL, 6, settings_id, true, false, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "ActionObjects" WHERE "Code" = 'SETTINGS_ROLES');

INSERT INTO "ActionObjects" ("Id", "Name", "Code", "Description", "ObjectType", "Route", "SortOrder", "ParentObjectId", "IsActive", "IsDeleted", "CreatedAt")
SELECT gen_random_uuid(), 'Preferences', 'SETTINGS_PREFERENCES', 'User preferences', 0, NULL, 7, settings_id, true, false, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "ActionObjects" WHERE "Code" = 'SETTINGS_PREFERENCES');

INSERT INTO "ActionObjects" ("Id", "Name", "Code", "Description", "ObjectType", "Route", "SortOrder", "ParentObjectId", "IsActive", "IsDeleted", "CreatedAt")
SELECT gen_random_uuid(), 'Access Control', 'SETTINGS_ACCESS_CONTROL', 'Access control settings', 0, NULL, 8, settings_id, true, false, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "ActionObjects" WHERE "Code" = 'SETTINGS_ACCESS_CONTROL');

END $$;

-- Seed ActionObjects for API Endpoints (ObjectType=1 / Url)
DO $$
DECLARE
    api_auth_id        UUID := gen_random_uuid();
    api_users_id       UUID := gen_random_uuid();
    api_roles_id       UUID := gen_random_uuid();
    api_departments_id UUID := gen_random_uuid();
    api_jobtitles_id   UUID := gen_random_uuid();
    api_orgs_id        UUID := gen_random_uuid();
    api_usergroups_id  UUID := gen_random_uuid();
    api_rbac_id        UUID := gen_random_uuid();
    api_forms_id       UUID := gen_random_uuid();
    api_graphs_id      UUID := gen_random_uuid();
    api_folders_id     UUID := gen_random_uuid();
    api_formquery_id   UUID := gen_random_uuid();
    api_grouprules_id  UUID := gen_random_uuid();
    api_groupperm_id   UUID := gen_random_uuid();
BEGIN

-- API endpoints are seeded automatically by EndpointSyncService on startup.
-- It scans all controllers via reflection and inserts missing ActionObjects (ObjectType=Url).

END $$;
