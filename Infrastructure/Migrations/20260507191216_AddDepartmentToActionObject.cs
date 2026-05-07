using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentToActionObject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "ActionObjects",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActionObjects_DepartmentId",
                table: "ActionObjects",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionObjects_Departments_DepartmentId",
                table: "ActionObjects",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActionObjects_Departments_DepartmentId",
                table: "ActionObjects");

            migrationBuilder.DropIndex(
                name: "IX_ActionObjects_DepartmentId",
                table: "ActionObjects");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "ActionObjects");
        }
    }
}
