using System;
using System.Collections.Generic;
using Core.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class dynamicPermissionAction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FolderPermissionId",
                table: "DynamicPermissionRules");

            migrationBuilder.RenameColumn(
                name: "WorkflowPermissionId",
                table: "DynamicPermissionRules",
                newName: "PermissionId");

            migrationBuilder.RenameColumn(
                name: "RequestPermissionId",
                table: "DynamicPermissionRules",
                newName: "ActionObjectId");

            migrationBuilder.CreateTable(
                name: "ActionObject",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ObjectType = table.Column<int>(type: "integer", nullable: false),
                    ParentObjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionObject", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActionObject_ActionObject_ParentObjectId",
                        column: x => x.ParentObjectId,
                        principalTable: "ActionObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GraphConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    View = table.Column<GraphViewConfig>(type: "jsonb", nullable: false),
                    Data = table.Column<GraphDataConfig>(type: "jsonb", nullable: false),
                    Meta = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("PK_GraphConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GraphConfigs_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GraphConfigs_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ActionObjectPermissionAssignment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionObjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssigneeType = table.Column<int>(type: "integer", nullable: false),
                    AssigneeId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_ActionObjectPermissionAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActionObjectPermissionAssignment_ActionObject_ActionObjectId",
                        column: x => x.ActionObjectId,
                        principalTable: "ActionObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActionObjectPermissionAssignment_AppPermissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "AppPermissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActionObjectPermissionAssignment_Organizations_Organization~",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ActionObjectPermissionAssignment_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GraphDataDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GraphConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<DataSourceDefinition>(type: "jsonb", nullable: false),
                    SeriesCalculations = table.Column<List<SeriesCalculation>>(type: "jsonb", nullable: false),
                    GlobalFilter = table.Column<FilterGroup>(type: "jsonb", nullable: true),
                    SortRules = table.Column<List<SortRule>>(type: "jsonb", nullable: true),
                    RowLimit = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_GraphDataDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GraphDataDefinitions_GraphConfigs_GraphConfigId",
                        column: x => x.GraphConfigId,
                        principalTable: "GraphConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GraphDataDefinitions_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GraphDataDefinitions_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DynamicPermissionRules_ActionObjectId",
                table: "DynamicPermissionRules",
                column: "ActionObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicPermissionRules_PermissionId",
                table: "DynamicPermissionRules",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionObject_ParentObjectId",
                table: "ActionObject",
                column: "ParentObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionObjectPermissionAssignment_ActionObjectId",
                table: "ActionObjectPermissionAssignment",
                column: "ActionObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionObjectPermissionAssignment_CreatedById",
                table: "ActionObjectPermissionAssignment",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ActionObjectPermissionAssignment_OrganizationId",
                table: "ActionObjectPermissionAssignment",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionObjectPermissionAssignment_PermissionId",
                table: "ActionObjectPermissionAssignment",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_GraphConfigs_CreatedById",
                table: "GraphConfigs",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_GraphConfigs_OrganizationId",
                table: "GraphConfigs",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_GraphDataDefinitions_CreatedById",
                table: "GraphDataDefinitions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_GraphDataDefinitions_GraphConfigId",
                table: "GraphDataDefinitions",
                column: "GraphConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_GraphDataDefinitions_OrganizationId",
                table: "GraphDataDefinitions",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicPermissionRules_ActionObject_ActionObjectId",
                table: "DynamicPermissionRules",
                column: "ActionObjectId",
                principalTable: "ActionObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicPermissionRules_AppPermissions_PermissionId",
                table: "DynamicPermissionRules",
                column: "PermissionId",
                principalTable: "AppPermissions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DynamicPermissionRules_ActionObject_ActionObjectId",
                table: "DynamicPermissionRules");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicPermissionRules_AppPermissions_PermissionId",
                table: "DynamicPermissionRules");

            migrationBuilder.DropTable(
                name: "ActionObjectPermissionAssignment");

            migrationBuilder.DropTable(
                name: "GraphDataDefinitions");

            migrationBuilder.DropTable(
                name: "ActionObject");

            migrationBuilder.DropTable(
                name: "GraphConfigs");

            migrationBuilder.DropIndex(
                name: "IX_DynamicPermissionRules_ActionObjectId",
                table: "DynamicPermissionRules");

            migrationBuilder.DropIndex(
                name: "IX_DynamicPermissionRules_PermissionId",
                table: "DynamicPermissionRules");

            migrationBuilder.RenameColumn(
                name: "PermissionId",
                table: "DynamicPermissionRules",
                newName: "WorkflowPermissionId");

            migrationBuilder.RenameColumn(
                name: "ActionObjectId",
                table: "DynamicPermissionRules",
                newName: "RequestPermissionId");

            migrationBuilder.AddColumn<Guid>(
                name: "FolderPermissionId",
                table: "DynamicPermissionRules",
                type: "uuid",
                nullable: true);
        }
    }
}
