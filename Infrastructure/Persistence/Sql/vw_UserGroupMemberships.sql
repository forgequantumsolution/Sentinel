-- ===================================================================
-- Helper: resolve a user's field value from a dot-path like "User.Role.Name"
-- ===================================================================
CREATE OR REPLACE FUNCTION resolve_user_field(p_user_id UUID, p_field TEXT)
RETURNS TEXT AS $$
DECLARE
    v_result TEXT;
BEGIN
    SELECT
        CASE p_field
            WHEN 'User.Role.Name'       THEN rol."Name"
            WHEN 'Role.Name'            THEN rol."Name"
            WHEN 'User.Department.Name' THEN dep."Name"
            WHEN 'Department.Name'      THEN dep."Name"
            WHEN 'User.Department.Code' THEN dep."Code"
            WHEN 'Department.Code'      THEN dep."Code"
            WHEN 'User.JobTitle.Title'  THEN jt."Title"
            WHEN 'JobTitle.Title'       THEN jt."Title"
            WHEN 'User.Location'        THEN u."Location"
            WHEN 'Location'             THEN u."Location"
            WHEN 'User.EmploymentType'  THEN u."EmploymentType"
            WHEN 'EmploymentType'       THEN u."EmploymentType"
            WHEN 'User.Division'        THEN u."Division"
            WHEN 'Division'             THEN u."Division"
            WHEN 'User.BusinessUnit'    THEN u."BusinessUnit"
            WHEN 'BusinessUnit'         THEN u."BusinessUnit"
            WHEN 'User.CostCenter'      THEN u."CostCenter"
            WHEN 'CostCenter'           THEN u."CostCenter"
            WHEN 'User.Name'            THEN u."Name"
            WHEN 'Name'                 THEN u."Name"
            WHEN 'User.Email'           THEN u."Email"
            WHEN 'Email'                THEN u."Email"
            WHEN 'User.EmployeeId'      THEN u."EmployeeId"
            WHEN 'EmployeeId'           THEN u."EmployeeId"
            WHEN 'User.HireDate'        THEN u."HireDate"::TEXT
            WHEN 'HireDate'             THEN u."HireDate"::TEXT
            WHEN 'User.Status'          THEN u."Status"::TEXT
            WHEN 'Status'               THEN u."Status"::TEXT
            ELSE NULL
        END
    INTO v_result
    FROM "Users"            u
    LEFT JOIN "Roles"       rol ON u."RoleId"       = rol."Id"
    LEFT JOIN "Departments" dep ON u."DepartmentId" = dep."Id"
    LEFT JOIN "JobTitles"   jt  ON u."JobTitleId"   = jt."Id"
    WHERE u."Id" = p_user_id;

    RETURN v_result;
END;
$$ LANGUAGE plpgsql STABLE;


-- ===================================================================
-- Recursive rule evaluator — mirrors BaseDynamicRule.Evaluate() in C#
-- Handles Simple, AND, OR at any nesting depth with short-circuiting
-- ===================================================================
CREATE OR REPLACE FUNCTION evaluate_grouping_rule(p_rule_id UUID, p_user_id UUID)
RETURNS BOOLEAN AS $$
DECLARE
    v_rule       RECORD;
    v_field_val  TEXT;
    v_lower_val  TEXT;
    v_lower_exp  TEXT;
    v_child      RECORD;
    v_child_res  BOOLEAN;
    v_all_match  BOOLEAN := true;
    v_any_match  BOOLEAN := false;
    v_has_child  BOOLEAN := false;
BEGIN
    SELECT "RuleType", "Field", "Operator", "Value"
    INTO v_rule
    FROM "DynamicGroupingRules"
    WHERE "Id" = p_rule_id AND "IsActive" = true AND "IsDeleted" = false;

    IF NOT FOUND THEN RETURN false; END IF;

    -- ---------------------------------------------------------------
    -- Simple rule: evaluate field against value
    -- ---------------------------------------------------------------
    IF v_rule."RuleType" = 0 THEN
        v_field_val := resolve_user_field(p_user_id, v_rule."Field");
        IF v_field_val IS NULL THEN RETURN false; END IF;

        v_lower_val := LOWER(v_field_val);
        v_lower_exp := LOWER(v_rule."Value");

        RETURN CASE v_rule."Operator"
            WHEN 0  THEN v_lower_val = v_lower_exp                                             -- Equals
            WHEN 1  THEN v_lower_val <> v_lower_exp                                            -- NotEquals
            WHEN 2  THEN v_lower_val LIKE '%' || v_lower_exp || '%'                            -- Contains
            WHEN 3  THEN v_lower_val LIKE v_lower_exp || '%'                                   -- StartsWith
            WHEN 4  THEN v_lower_val LIKE '%' || v_lower_exp                                   -- EndsWith
            WHEN 5  THEN v_lower_val = ANY(string_to_array(v_lower_exp, ','))                  -- In
            WHEN 6  THEN NOT (v_lower_val = ANY(string_to_array(v_lower_exp, ',')))            -- NotIn
            WHEN 7  THEN v_field_val > v_rule."Value"                                          -- GreaterThan
            WHEN 8  THEN v_field_val < v_rule."Value"                                          -- LessThan
            WHEN 9  THEN v_field_val >= v_rule."Value"                                         -- GreaterThanOrEqual
            WHEN 10 THEN v_field_val <= v_rule."Value"                                         -- LessThanOrEqual
            ELSE false
        END;
    END IF;

    -- ---------------------------------------------------------------
    -- Composite rule: recurse into children with short-circuit
    -- ---------------------------------------------------------------
    FOR v_child IN
        SELECT "Id"
        FROM "DynamicGroupingRules"
        WHERE "ParentRuleId" = p_rule_id AND "IsActive" = true AND "IsDeleted" = false
    LOOP
        v_has_child := true;
        v_child_res := evaluate_grouping_rule(v_child."Id", p_user_id);

        IF v_rule."RuleType" = 1 THEN       -- AND
            IF NOT v_child_res THEN RETURN false; END IF;
        ELSIF v_rule."RuleType" = 2 THEN     -- OR
            IF v_child_res THEN RETURN true; END IF;
        END IF;
    END LOOP;

    IF NOT v_has_child THEN RETURN false; END IF;

    -- AND: all passed (none short-circuited false)
    -- OR:  none passed (none short-circuited true)
    RETURN v_rule."RuleType" = 1;
END;
$$ LANGUAGE plpgsql STABLE;


-- ===================================================================
-- The VIEW — resolves the full chain:
--   User → (grouping rules) → UserGroup → DynamicGroupObjectPermission
--        → ActionObjectPermissionSet → ActionObjectPermissionSetItem
--
-- Output: UserId, UserGroupId, RuleId, ActionObjectId, PermissionId, OrganizationId
-- ===================================================================
CREATE OR REPLACE VIEW "vw_UserGroupMemberships" AS
SELECT
    m."UserId",
    m."UserGroupId",
    m."RuleId",
    aops."ActionObjectId",
    aopsi."PermissionId",
    m."OrganizationId"
FROM (
    -- Base: users matched to groups via grouping rules
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
) m
-- Join permissions assigned to the group
LEFT JOIN "DynamicGroupObjectPermissions" dpr
    ON dpr."UserGroupId" = m."UserGroupId"
   AND dpr."IsActive"    = true
   AND dpr."IsDeleted"   = false
   AND dpr."IsAllowed"   = true
LEFT JOIN "ActionObjectPermissionSets" aops
    ON aops."DynamicGroupObjectPermissionId" = dpr."Id"
   AND aops."IsActive"  = true
   AND aops."IsDeleted"  = false
LEFT JOIN "ActionObjectPermissionSetItems" aopsi
    ON aopsi."ActionObjectPermissionSetId" = aops."Id"
   AND aopsi."IsActive"  = true
   AND aopsi."IsDeleted"  = false;
