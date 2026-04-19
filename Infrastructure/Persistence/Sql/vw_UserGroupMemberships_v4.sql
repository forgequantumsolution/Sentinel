-- ===================================================================
-- v4: Two changes consolidated
--   1. resolve_user_field: adds User.Organization.Name / Code / OrganizationId
--   2. vw_UserGroupMemberships: admin role override — groups linked to roles
--      named 'super-admin', 'sys-admin', 'admin' get ALL ActionObjects ×
--      ALL Permissions automatically (no explicit assignment needed).
-- ===================================================================

CREATE OR REPLACE FUNCTION resolve_user_field(p_user_id UUID, p_field TEXT)
RETURNS TEXT AS $$
DECLARE
    v_result TEXT;
BEGIN
    SELECT
        CASE p_field
            WHEN 'User.Role.Name'         THEN rol."Name"
            WHEN 'Role.Name'              THEN rol."Name"
            WHEN 'User.Department.Name'   THEN dep."Name"
            WHEN 'Department.Name'        THEN dep."Name"
            WHEN 'User.Department.Code'   THEN dep."Code"
            WHEN 'Department.Code'        THEN dep."Code"
            WHEN 'User.DepartmentId'      THEN u."DepartmentId"::TEXT
            WHEN 'DepartmentId'           THEN u."DepartmentId"::TEXT
            WHEN 'User.RoleId'            THEN u."RoleId"::TEXT
            WHEN 'RoleId'                 THEN u."RoleId"::TEXT
            WHEN 'User.JobTitle.Title'    THEN jt."Title"
            WHEN 'JobTitle.Title'         THEN jt."Title"
            WHEN 'User.Organization.Name' THEN org."Name"
            WHEN 'Organization.Name'      THEN org."Name"
            WHEN 'User.Organization.Code' THEN org."Code"
            WHEN 'Organization.Code'      THEN org."Code"
            WHEN 'User.OrganizationId'    THEN u."OrganizationId"::TEXT
            WHEN 'OrganizationId'         THEN u."OrganizationId"::TEXT
            WHEN 'User.Location'          THEN u."Location"
            WHEN 'Location'               THEN u."Location"
            WHEN 'User.EmploymentType'    THEN u."EmploymentType"
            WHEN 'EmploymentType'         THEN u."EmploymentType"
            WHEN 'User.Division'          THEN u."Division"
            WHEN 'Division'               THEN u."Division"
            WHEN 'User.BusinessUnit'      THEN u."BusinessUnit"
            WHEN 'BusinessUnit'           THEN u."BusinessUnit"
            WHEN 'User.CostCenter'        THEN u."CostCenter"
            WHEN 'CostCenter'             THEN u."CostCenter"
            WHEN 'User.Name'              THEN u."Name"
            WHEN 'Name'                   THEN u."Name"
            WHEN 'User.Email'             THEN u."Email"
            WHEN 'Email'                  THEN u."Email"
            WHEN 'User.EmployeeId'        THEN u."EmployeeId"
            WHEN 'EmployeeId'             THEN u."EmployeeId"
            WHEN 'User.HireDate'          THEN u."HireDate"::TEXT
            WHEN 'HireDate'               THEN u."HireDate"::TEXT
            WHEN 'User.Status'            THEN u."Status"::TEXT
            WHEN 'Status'                 THEN u."Status"::TEXT
            ELSE NULL
        END
    INTO v_result
    FROM "Users"             u
    LEFT JOIN "Roles"        rol ON u."RoleId"         = rol."Id"
    LEFT JOIN "Departments"  dep ON u."DepartmentId"   = dep."Id"
    LEFT JOIN "JobTitles"    jt  ON u."JobTitleId"     = jt."Id"
    LEFT JOIN "Organizations" org ON u."OrganizationId" = org."Id"
    WHERE u."Id" = p_user_id;

    RETURN v_result;
END;
$$ LANGUAGE plpgsql STABLE;


-- ===================================================================
-- The VIEW v4 — full chain plus admin role override:
--   1. Regular path: User → Group → ActionObjectPermissionAssignment
--   2. Admin override: Groups linked to super-admin/sys-admin/admin roles
--      get ALL ActionObjects × ALL Permissions automatically.
--
-- Output: UserId, UserGroupId, RuleId, ActionObjectId, PermissionId, OrganizationId
-- ===================================================================
CREATE OR REPLACE VIEW "vw_UserGroupMemberships" AS

WITH base_membership AS (
    -- Users matched to groups via grouping rules
    SELECT
        u."Id"              AS "UserId",
        r."UserGroupId",
        r."Id"              AS "RuleId",
        u."OrganizationId"
    FROM "DynamicGroupingRules" r
    CROSS JOIN "Users" u
    WHERE r."ParentRuleId" IS NULL
      AND r."IsActive"     = true  AND r."IsDeleted"  = false
      AND r."AutoAssign"   = true
      AND u."IsActive"     = true  AND u."IsDeleted"  = false
      AND u."OrganizationId" = r."OrganizationId"
      AND evaluate_grouping_rule(r."Id", u."Id") = true
)

-- ── Path 1: regular explicit permission assignments ──────────────────
SELECT
    m."UserId",
    m."UserGroupId",
    m."RuleId",
    a."ActionObjectId",
    a."PermissionId",
    m."OrganizationId"
FROM base_membership m
LEFT JOIN "UserGroups" g  ON g."Id"  = m."UserGroupId"
LEFT JOIN "Roles"      r2 ON r2."Id" = g."RoleId"
LEFT JOIN "ActionObjectPermissionAssignments" a
    ON a."AssigneeType" = 2          -- AssigneeType.Group
   AND a."AssigneeId"   = m."UserGroupId"
   AND a."IsActive"     = true
   AND a."IsDeleted"    = false
WHERE r2."Name" IS NULL
   OR r2."Name" NOT IN ('super-admin', 'sys-admin', 'admin')

UNION

-- ── Path 2: admin role override — all ActionObjects × all Permissions ──
SELECT
    m."UserId",
    m."UserGroupId",
    m."RuleId",
    ao."Id"  AS "ActionObjectId",
    p."Id"   AS "PermissionId",
    m."OrganizationId"
FROM base_membership m
INNER JOIN "UserGroups" g  ON g."Id"  = m."UserGroupId"
INNER JOIN "Roles"      r2 ON r2."Id" = g."RoleId"
                          AND r2."Name" IN ('super-admin', 'sys-admin', 'admin')
CROSS JOIN "ActionObjects" ao
CROSS JOIN "AppPermissions" p
WHERE ao."IsActive" = true AND ao."IsDeleted" = false
  AND p."IsActive"  = true AND p."IsDeleted"  = false;
