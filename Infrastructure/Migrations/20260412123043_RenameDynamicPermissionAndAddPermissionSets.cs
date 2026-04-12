using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameDynamicPermissionAndAddPermissionSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DynamicPermissionRules");

            migrationBuilder.CreateTable(
                name: "DynamicGroupObjectPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UserGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_DynamicGroupObjectPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DynamicGroupObjectPermissions_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DynamicGroupObjectPermissions_UserGroups_UserGroupId",
                        column: x => x.UserGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DynamicGroupObjectPermissions_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ActionObjectPermissionSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DynamicGroupObjectPermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionObjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionObjectPermissionSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActionObjectPermissionSets_ActionObjects_ActionObjectId",
                        column: x => x.ActionObjectId,
                        principalTable: "ActionObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActionObjectPermissionSets_DynamicGroupObjectPermissions_Dy~",
                        column: x => x.DynamicGroupObjectPermissionId,
                        principalTable: "DynamicGroupObjectPermissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActionObjectPermissionSetItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionObjectPermissionSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionObjectPermissionSetItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActionObjectPermissionSetItems_ActionObjectPermissionSets_A~",
                        column: x => x.ActionObjectPermissionSetId,
                        principalTable: "ActionObjectPermissionSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActionObjectPermissionSetItems_AppPermissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "AppPermissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActionObjectPermissionSetItems_ActionObjectPermissionSetId",
                table: "ActionObjectPermissionSetItems",
                column: "ActionObjectPermissionSetId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionObjectPermissionSetItems_PermissionId",
                table: "ActionObjectPermissionSetItems",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionObjectPermissionSets_ActionObjectId",
                table: "ActionObjectPermissionSets",
                column: "ActionObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionObjectPermissionSets_DynamicGroupObjectPermissionId",
                table: "ActionObjectPermissionSets",
                column: "DynamicGroupObjectPermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicGroupObjectPermissions_CreatedById",
                table: "DynamicGroupObjectPermissions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicGroupObjectPermissions_OrganizationId",
                table: "DynamicGroupObjectPermissions",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicGroupObjectPermissions_UserGroupId",
                table: "DynamicGroupObjectPermissions",
                column: "UserGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActionObjectPermissionSetItems");

            migrationBuilder.DropTable(
                name: "ActionObjectPermissionSets");

            migrationBuilder.DropTable(
                name: "DynamicGroupObjectPermissions");

            migrationBuilder.CreateTable(
                name: "DynamicPermissionRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionObjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ParentRuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Field = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsDynamicValue = table.Column<bool>(type: "boolean", nullable: false),
                    IsInheritable = table.Column<bool>(type: "boolean", nullable: false),
                    IsInherited = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Operator = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    RuleType = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicPermissionRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DynamicPermissionRules_ActionObjects_ActionObjectId",
                        column: x => x.ActionObjectId,
                        principalTable: "ActionObjects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DynamicPermissionRules_AppPermissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "AppPermissions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DynamicPermissionRules_DynamicPermissionRules_ParentRuleId",
                        column: x => x.ParentRuleId,
                        principalTable: "DynamicPermissionRules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DynamicPermissionRules_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DynamicPermissionRules_UserGroups_UserGroupId",
                        column: x => x.UserGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DynamicPermissionRules_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DynamicPermissionRules_ActionObjectId",
                table: "DynamicPermissionRules",
                column: "ActionObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicPermissionRules_CreatedById",
                table: "DynamicPermissionRules",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicPermissionRules_OrganizationId",
                table: "DynamicPermissionRules",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicPermissionRules_ParentRuleId",
                table: "DynamicPermissionRules",
                column: "ParentRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicPermissionRules_PermissionId",
                table: "DynamicPermissionRules",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicPermissionRules_UserGroupId",
                table: "DynamicPermissionRules",
                column: "UserGroupId");
        }
    }
}
