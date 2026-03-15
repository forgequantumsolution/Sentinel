using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RecordEAVFolderStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActionObject_ActionObject_ParentObjectId",
                table: "ActionObject");

            migrationBuilder.DropForeignKey(
                name: "FK_ActionObjectPermissionAssignment_ActionObject_ActionObjectId",
                table: "ActionObjectPermissionAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicPermissionRules_ActionObject_ActionObjectId",
                table: "DynamicPermissionRules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActionObject",
                table: "ActionObject");

            migrationBuilder.RenameTable(
                name: "ActionObject",
                newName: "ActionObjects");

            migrationBuilder.RenameIndex(
                name: "IX_ActionObject_ParentObjectId",
                table: "ActionObjects",
                newName: "IX_ActionObjects_ParentObjectId");

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "ActionObjects",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Route",
                table: "ActionObjects",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "ActionObjects",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActionObjects",
                table: "ActionObjects",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ActionObjects_Route",
                table: "ActionObjects",
                column: "Route",
                unique: true,
                filter: "\"Route\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionObjectPermissionAssignment_ActionObjects_ActionObject~",
                table: "ActionObjectPermissionAssignment",
                column: "ActionObjectId",
                principalTable: "ActionObjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ActionObjects_ActionObjects_ParentObjectId",
                table: "ActionObjects",
                column: "ParentObjectId",
                principalTable: "ActionObjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicPermissionRules_ActionObjects_ActionObjectId",
                table: "DynamicPermissionRules",
                column: "ActionObjectId",
                principalTable: "ActionObjects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActionObjectPermissionAssignment_ActionObjects_ActionObject~",
                table: "ActionObjectPermissionAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_ActionObjects_ActionObjects_ParentObjectId",
                table: "ActionObjects");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicPermissionRules_ActionObjects_ActionObjectId",
                table: "DynamicPermissionRules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActionObjects",
                table: "ActionObjects");

            migrationBuilder.DropIndex(
                name: "IX_ActionObjects_Route",
                table: "ActionObjects");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "ActionObjects");

            migrationBuilder.DropColumn(
                name: "Route",
                table: "ActionObjects");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "ActionObjects");

            migrationBuilder.RenameTable(
                name: "ActionObjects",
                newName: "ActionObject");

            migrationBuilder.RenameIndex(
                name: "IX_ActionObjects_ParentObjectId",
                table: "ActionObject",
                newName: "IX_ActionObject_ParentObjectId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActionObject",
                table: "ActionObject",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionObject_ActionObject_ParentObjectId",
                table: "ActionObject",
                column: "ParentObjectId",
                principalTable: "ActionObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionObjectPermissionAssignment_ActionObject_ActionObjectId",
                table: "ActionObjectPermissionAssignment",
                column: "ActionObjectId",
                principalTable: "ActionObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicPermissionRules_ActionObject_ActionObjectId",
                table: "DynamicPermissionRules",
                column: "ActionObjectId",
                principalTable: "ActionObject",
                principalColumn: "Id");
        }
    }
}
