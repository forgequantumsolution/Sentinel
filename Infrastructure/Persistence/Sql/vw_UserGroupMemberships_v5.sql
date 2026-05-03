-- ===================================================================
-- v5: vw_UserGroupMemberships gains group-only paths.
--
-- Why: the v4 view only emits rows when a User matches a group's grouping
-- rules. That broke group-keyed permission queries (e.g. GetGroupAssignmentsAsync) —
-- a group with assignments but no matched users showed nothing.
--
-- This revision adds two extra UNION branches that emit each group's
-- permissions independently of user matching, with UserId / RuleId NULL.
-- User-keyed queries (Where UserId == userId) still see only Paths 1+2;
-- group-keyed queries (Where UserGroupId == groupId) now see all 4 and
-- rely on the existing application-level .Distinct() to dedupe overlap.
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

-- ── Path 1: regular explicit permission assignments (per matched user) ──
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

-- ── Path 2: admin role override per matched user — all AOs × all Perms ──
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
  AND p."IsActive"  = true AND p."IsDeleted"  = false

UNION

-- ── Path 3: group-only — every non-admin group's assignments, no user required ──
SELECT
    NULL::uuid          AS "UserId",
    g."Id"              AS "UserGroupId",
    NULL::uuid          AS "RuleId",
    a."ActionObjectId",
    a."PermissionId",
    g."OrganizationId"
FROM "UserGroups" g
LEFT JOIN "Roles" r3 ON r3."Id" = g."RoleId"
INNER JOIN "ActionObjectPermissionAssignments" a
    ON a."AssigneeType" = 2          -- AssigneeType.Group
   AND a."AssigneeId"   = g."Id"
   AND a."IsActive"     = true
   AND a."IsDeleted"    = false
WHERE g."IsActive" = true AND g."IsDeleted" = false
  AND (r3."Name" IS NULL OR r3."Name" NOT IN ('super-admin', 'sys-admin', 'admin'))

UNION

-- ── Path 4: group-only admin override — all AOs × all Perms, no user required ──
SELECT
    NULL::uuid          AS "UserId",
    g."Id"              AS "UserGroupId",
    NULL::uuid          AS "RuleId",
    ao."Id"             AS "ActionObjectId",
    p."Id"              AS "PermissionId",
    g."OrganizationId"
FROM "UserGroups" g
INNER JOIN "Roles" r4 ON r4."Id" = g."RoleId"
                     AND r4."Name" IN ('super-admin', 'sys-admin', 'admin')
CROSS JOIN "ActionObjects" ao
CROSS JOIN "AppPermissions" p
WHERE g."IsActive"  = true AND g."IsDeleted"  = false
  AND ao."IsActive" = true AND ao."IsDeleted" = false
  AND p."IsActive"  = true AND p."IsDeleted"  = false;
