using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GraphFolder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ActionObjectId",
                table: "GraphConfigs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GraphConfigs_ActionObjectId",
                table: "GraphConfigs",
                column: "ActionObjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_GraphConfigs_ActionObjects_ActionObjectId",
                table: "GraphConfigs",
                column: "ActionObjectId",
                principalTable: "ActionObjects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GraphConfigs_ActionObjects_ActionObjectId",
                table: "GraphConfigs");

            migrationBuilder.DropIndex(
                name: "IX_GraphConfigs_ActionObjectId",
                table: "GraphConfigs");

            migrationBuilder.DropColumn(
                name: "ActionObjectId",
                table: "GraphConfigs");
        }
    }
}
