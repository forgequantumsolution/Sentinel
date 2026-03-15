using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RecordEAV : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DynamicFormRecordValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FormId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_DynamicFormRecordValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DynamicFormRecordValues_DynamicFormFieldDefinitions_FieldDe~",
                        column: x => x.FieldDefinitionId,
                        principalTable: "DynamicFormFieldDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DynamicFormRecordValues_DynamicFormSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "DynamicFormSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DynamicFormRecordValues_DynamicForms_FormId",
                        column: x => x.FormId,
                        principalTable: "DynamicForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DynamicFormRecordValues_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DynamicFormRecordValues_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormRecordValues_CreatedById",
                table: "DynamicFormRecordValues",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormRecordValues_FieldDefinitionId",
                table: "DynamicFormRecordValues",
                column: "FieldDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormRecordValues_FormId_FieldDefinitionId",
                table: "DynamicFormRecordValues",
                columns: new[] { "FormId", "FieldDefinitionId" });

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormRecordValues_OrganizationId",
                table: "DynamicFormRecordValues",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormRecordValues_SubmissionId_FieldDefinitionId",
                table: "DynamicFormRecordValues",
                columns: new[] { "SubmissionId", "FieldDefinitionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DynamicFormRecordValues");
        }
    }
}
