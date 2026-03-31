using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class pOrg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentOrganizationId",
                table: "Organizations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_ParentOrganizationId",
                table: "Organizations",
                column: "ParentOrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Organizations_ParentOrganizationId",
                table: "Organizations",
                column: "ParentOrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Organizations_ParentOrganizationId",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_ParentOrganizationId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "ParentOrganizationId",
                table: "Organizations");
        }
    }
}
