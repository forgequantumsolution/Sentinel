using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class organization_filtering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "UserGroups",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Roles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Permissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "JobTitles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "DynamicPermissionRules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "DynamicGroupingRules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "DynamicFormSubmissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "DynamicForms",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Departments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DynamicFormFieldDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FormId = table.Column<Guid>(type: "uuid", nullable: false),
                    ColumnName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FieldName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FieldType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    ValidationRules = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicFormFieldDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DynamicFormFieldDefinitions_DynamicForms_FormId",
                        column: x => x.FormId,
                        principalTable: "DynamicForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DynamicFormFieldDefinitions_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DynamicFormFieldDefinitions_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DynamicFormRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FormId = table.Column<Guid>(type: "uuid", nullable: false),
                    Field1 = table.Column<string>(type: "text", nullable: true),
                    Field2 = table.Column<string>(type: "text", nullable: true),
                    Field3 = table.Column<string>(type: "text", nullable: true),
                    Field4 = table.Column<string>(type: "text", nullable: true),
                    Field5 = table.Column<string>(type: "text", nullable: true),
                    Field6 = table.Column<string>(type: "text", nullable: true),
                    Field7 = table.Column<string>(type: "text", nullable: true),
                    Field8 = table.Column<string>(type: "text", nullable: true),
                    Field9 = table.Column<string>(type: "text", nullable: true),
                    Field10 = table.Column<string>(type: "text", nullable: true),
                    Field11 = table.Column<string>(type: "text", nullable: true),
                    Field12 = table.Column<string>(type: "text", nullable: true),
                    Field13 = table.Column<string>(type: "text", nullable: true),
                    Field14 = table.Column<string>(type: "text", nullable: true),
                    Field15 = table.Column<string>(type: "text", nullable: true),
                    Field16 = table.Column<string>(type: "text", nullable: true),
                    Field17 = table.Column<string>(type: "text", nullable: true),
                    Field18 = table.Column<string>(type: "text", nullable: true),
                    Field19 = table.Column<string>(type: "text", nullable: true),
                    Field20 = table.Column<string>(type: "text", nullable: true),
                    Field21 = table.Column<string>(type: "text", nullable: true),
                    Field22 = table.Column<string>(type: "text", nullable: true),
                    Field23 = table.Column<string>(type: "text", nullable: true),
                    Field24 = table.Column<string>(type: "text", nullable: true),
                    Field25 = table.Column<string>(type: "text", nullable: true),
                    Field26 = table.Column<string>(type: "text", nullable: true),
                    Field27 = table.Column<string>(type: "text", nullable: true),
                    Field28 = table.Column<string>(type: "text", nullable: true),
                    Field29 = table.Column<string>(type: "text", nullable: true),
                    Field30 = table.Column<string>(type: "text", nullable: true),
                    Field31 = table.Column<string>(type: "text", nullable: true),
                    Field32 = table.Column<string>(type: "text", nullable: true),
                    Field33 = table.Column<string>(type: "text", nullable: true),
                    Field34 = table.Column<string>(type: "text", nullable: true),
                    Field35 = table.Column<string>(type: "text", nullable: true),
                    Field36 = table.Column<string>(type: "text", nullable: true),
                    Field37 = table.Column<string>(type: "text", nullable: true),
                    Field38 = table.Column<string>(type: "text", nullable: true),
                    Field39 = table.Column<string>(type: "text", nullable: true),
                    Field40 = table.Column<string>(type: "text", nullable: true),
                    Field41 = table.Column<string>(type: "text", nullable: true),
                    Field42 = table.Column<string>(type: "text", nullable: true),
                    Field43 = table.Column<string>(type: "text", nullable: true),
                    Field44 = table.Column<string>(type: "text", nullable: true),
                    Field45 = table.Column<string>(type: "text", nullable: true),
                    Field46 = table.Column<string>(type: "text", nullable: true),
                    Field47 = table.Column<string>(type: "text", nullable: true),
                    Field48 = table.Column<string>(type: "text", nullable: true),
                    Field49 = table.Column<string>(type: "text", nullable: true),
                    Field50 = table.Column<string>(type: "text", nullable: true),
                    Field51 = table.Column<string>(type: "text", nullable: true),
                    Field52 = table.Column<string>(type: "text", nullable: true),
                    Field53 = table.Column<string>(type: "text", nullable: true),
                    Field54 = table.Column<string>(type: "text", nullable: true),
                    Field55 = table.Column<string>(type: "text", nullable: true),
                    Field56 = table.Column<string>(type: "text", nullable: true),
                    Field57 = table.Column<string>(type: "text", nullable: true),
                    Field58 = table.Column<string>(type: "text", nullable: true),
                    Field59 = table.Column<string>(type: "text", nullable: true),
                    Field60 = table.Column<string>(type: "text", nullable: true),
                    Field61 = table.Column<string>(type: "text", nullable: true),
                    Field62 = table.Column<string>(type: "text", nullable: true),
                    Field63 = table.Column<string>(type: "text", nullable: true),
                    Field64 = table.Column<string>(type: "text", nullable: true),
                    Field65 = table.Column<string>(type: "text", nullable: true),
                    Field66 = table.Column<string>(type: "text", nullable: true),
                    Field67 = table.Column<string>(type: "text", nullable: true),
                    Field68 = table.Column<string>(type: "text", nullable: true),
                    Field69 = table.Column<string>(type: "text", nullable: true),
                    Field70 = table.Column<string>(type: "text", nullable: true),
                    Field71 = table.Column<string>(type: "text", nullable: true),
                    Field72 = table.Column<string>(type: "text", nullable: true),
                    Field73 = table.Column<string>(type: "text", nullable: true),
                    Field74 = table.Column<string>(type: "text", nullable: true),
                    Field75 = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicFormRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DynamicFormRecords_DynamicForms_FormId",
                        column: x => x.FormId,
                        principalTable: "DynamicForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DynamicFormRecords_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DynamicFormRecords_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationId",
                table: "Users",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_OrganizationId",
                table: "UserGroups",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_OrganizationId",
                table: "Roles",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_OrganizationId",
                table: "Permissions",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTitles_OrganizationId",
                table: "JobTitles",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicPermissionRules_OrganizationId",
                table: "DynamicPermissionRules",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicGroupingRules_OrganizationId",
                table: "DynamicGroupingRules",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormSubmissions_OrganizationId",
                table: "DynamicFormSubmissions",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicForms_OrganizationId",
                table: "DynamicForms",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_OrganizationId",
                table: "Departments",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormFieldDefinitions_CreatedById",
                table: "DynamicFormFieldDefinitions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormFieldDefinitions_FormId",
                table: "DynamicFormFieldDefinitions",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormFieldDefinitions_OrganizationId",
                table: "DynamicFormFieldDefinitions",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormRecords_CreatedById",
                table: "DynamicFormRecords",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormRecords_FormId",
                table: "DynamicFormRecords",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormRecords_OrganizationId",
                table: "DynamicFormRecords",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Organizations_OrganizationId",
                table: "Departments",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicForms_Organizations_OrganizationId",
                table: "DynamicForms",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormSubmissions_Organizations_OrganizationId",
                table: "DynamicFormSubmissions",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicGroupingRules_Organizations_OrganizationId",
                table: "DynamicGroupingRules",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicPermissionRules_Organizations_OrganizationId",
                table: "DynamicPermissionRules",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_JobTitles_Organizations_OrganizationId",
                table: "JobTitles",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Organizations_OrganizationId",
                table: "Permissions",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Organizations_OrganizationId",
                table: "Roles",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserGroups_Organizations_OrganizationId",
                table: "UserGroups",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                table: "Users",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Organizations_OrganizationId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicForms_Organizations_OrganizationId",
                table: "DynamicForms");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormSubmissions_Organizations_OrganizationId",
                table: "DynamicFormSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicGroupingRules_Organizations_OrganizationId",
                table: "DynamicGroupingRules");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicPermissionRules_Organizations_OrganizationId",
                table: "DynamicPermissionRules");

            migrationBuilder.DropForeignKey(
                name: "FK_JobTitles_Organizations_OrganizationId",
                table: "JobTitles");

            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Organizations_OrganizationId",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Organizations_OrganizationId",
                table: "Roles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserGroups_Organizations_OrganizationId",
                table: "UserGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "DynamicFormFieldDefinitions");

            migrationBuilder.DropTable(
                name: "DynamicFormRecords");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Users_OrganizationId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserGroups_OrganizationId",
                table: "UserGroups");

            migrationBuilder.DropIndex(
                name: "IX_Roles_OrganizationId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_OrganizationId",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_JobTitles_OrganizationId",
                table: "JobTitles");

            migrationBuilder.DropIndex(
                name: "IX_DynamicPermissionRules_OrganizationId",
                table: "DynamicPermissionRules");

            migrationBuilder.DropIndex(
                name: "IX_DynamicGroupingRules_OrganizationId",
                table: "DynamicGroupingRules");

            migrationBuilder.DropIndex(
                name: "IX_DynamicFormSubmissions_OrganizationId",
                table: "DynamicFormSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_DynamicForms_OrganizationId",
                table: "DynamicForms");

            migrationBuilder.DropIndex(
                name: "IX_Departments_OrganizationId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "UserGroups");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "JobTitles");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "DynamicPermissionRules");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "DynamicGroupingRules");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "DynamicFormSubmissions");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "DynamicForms");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Departments");
        }
    }
}
