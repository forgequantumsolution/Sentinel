using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserGroupMembershipView_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("Infrastructure.Persistence.Sql.vw_UserGroupMemberships_v2.sql")!;
            using var reader = new System.IO.StreamReader(stream);
            migrationBuilder.Sql(reader.ReadToEnd());
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS ""vw_UserGroupMemberships"";");
        }
    }
}
