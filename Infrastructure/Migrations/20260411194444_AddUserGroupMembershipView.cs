using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserGroupMembershipView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The VIEW creation was moved to a later migration
            // (DynamicGroupObjectPermissions_And_ActionObjectOrgFilter) after the
            // DynamicPermissionRules table was renamed to DynamicGroupObjectPermissions.
            // This migration is intentionally empty to preserve history ordering.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS ""vw_UserGroupMemberships"";");
        }
    }
}
