using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class linking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActionObjectPermissionAssignment_ActionObjects_ActionObject~",
                table: "ActionObjectPermissionAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_ActionObjectPermissionAssignment_AppPermissions_PermissionId",
                table: "ActionObjectPermissionAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_ActionObjectPermissionAssignment_Organizations_Organization~",
                table: "ActionObjectPermissionAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_ActionObjectPermissionAssignment_Users_CreatedById",
                table: "ActionObjectPermissionAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_ActionObjects_Organizations_OrganizationId",
                table: "ActionObjects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActionObjectPermissionAssignment",
                table: "ActionObjectPermissionAssignment");

            migrationBuilder.RenameTable(
                name: "ActionObjectPermissionAssignment",
                newName: "ActionObjectPermissionAssignments");

            migrationBuilder.RenameIndex(
                name: "IX_ActionObjectPermissionAssignment_PermissionId",
                table: "ActionObjectPermissionAssignments",
                newName: "IX_ActionObjectPermissionAssignments_PermissionId");

            migrationBuilder.RenameIndex(
                name: "IX_ActionObjectPermissionAssignment_OrganizationId",
                table: "ActionObjectPermissionAssignments",
                newName: "IX_ActionObjectPermissionAssignments_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_ActionObjectPermissionAssignment_CreatedById",
                table: "ActionObjectPermissionAssignments",
                newName: "IX_ActionObjectPermissionAssignments_CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_ActionObjectPermissionAssignment_ActionObjectId",
                table: "ActionObjectPermissionAssignments",
                newName: "IX_ActionObjectPermissionAssignments_ActionObjectId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActionObjectPermissionAssignments",
                table: "ActionObjectPermissionAssignments",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ActionObjects_Code",
                table: "ActionObjects",
                column: "Code",
                unique: true,
                filter: "\"Code\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionObjectPermissionAssignments_ActionObjects_ActionObjec~",
                table: "ActionObjectPermissionAssignments",
                column: "ActionObjectId",
                principalTable: "ActionObjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ActionObjectPermissionAssignments_AppPermissions_Permission~",
                table: "ActionObjectPermissionAssignments",
                column: "PermissionId",
                principalTable: "AppPermissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ActionObjectPermissionAssignments_Organizations_Organizatio~",
                table: "ActionObjectPermissionAssignments",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionObjectPermissionAssignments_Users_CreatedById",
                table: "ActionObjectPermissionAssignments",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionObjects_Organizations_OrganizationId",
                table: "ActionObjects",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActionObjectPermissionAssignments_ActionObjects_ActionObjec~",
                table: "ActionObjectPermissionAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ActionObjectPermissionAssignments_AppPermissions_Permission~",
                table: "ActionObjectPermissionAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ActionObjectPermissionAssignments_Organizations_Organizatio~",
                table: "ActionObjectPermissionAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ActionObjectPermissionAssignments_Users_CreatedById",
                table: "ActionObjectPermissionAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ActionObjects_Organizations_OrganizationId",
                table: "ActionObjects");

            migrationBuilder.DropIndex(
                name: "IX_ActionObjects_Code",
                table: "ActionObjects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActionObjectPermissionAssignments",
                table: "ActionObjectPermissionAssignments");

            migrationBuilder.RenameTable(
                name: "ActionObjectPermissionAssignments",
                newName: "ActionObjectPermissionAssignment");

            migrationBuilder.RenameIndex(
                name: "IX_ActionObjectPermissionAssignments_PermissionId",
                table: "ActionObjectPermissionAssignment",
                newName: "IX_ActionObjectPermissionAssignment_PermissionId");

            migrationBuilder.RenameIndex(
                name: "IX_ActionObjectPermissionAssignments_OrganizationId",
                table: "ActionObjectPermissionAssignment",
                newName: "IX_ActionObjectPermissionAssignment_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_ActionObjectPermissionAssignments_CreatedById",
                table: "ActionObjectPermissionAssignment",
                newName: "IX_ActionObjectPermissionAssignment_CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_ActionObjectPermissionAssignments_ActionObjectId",
                table: "ActionObjectPermissionAssignment",
                newName: "IX_ActionObjectPermissionAssignment_ActionObjectId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActionObjectPermissionAssignment",
                table: "ActionObjectPermissionAssignment",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionObjectPermissionAssignment_ActionObjects_ActionObject~",
                table: "ActionObjectPermissionAssignment",
                column: "ActionObjectId",
                principalTable: "ActionObjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ActionObjectPermissionAssignment_AppPermissions_PermissionId",
                table: "ActionObjectPermissionAssignment",
                column: "PermissionId",
                principalTable: "AppPermissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ActionObjectPermissionAssignment_Organizations_Organization~",
                table: "ActionObjectPermissionAssignment",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionObjectPermissionAssignment_Users_CreatedById",
                table: "ActionObjectPermissionAssignment",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionObjects_Organizations_OrganizationId",
                table: "ActionObjects",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");
        }
    }
}
