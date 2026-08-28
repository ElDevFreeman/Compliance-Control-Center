using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceControlCenter.Web.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCtpatCatalogAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ExternalId",
                table: "CCC_CTPAT_Questions",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CCC_CTPAT_Questions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CCC_CTPAT_Questions",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "CCC_CTPAT_Questions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "CCC_CTPAT_Questions",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CCC_CTPAT_Guidance",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CCC_CTPAT_Guidance",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "CCC_CTPAT_Guidance",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "CCC_CTPAT_Guidance",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CCC_CTPAT_Questions_IsActive",
                table: "CCC_CTPAT_Questions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CCC_CTPAT_Guidance_IsActive",
                table: "CCC_CTPAT_Guidance",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CCC_CTPAT_Questions_IsActive",
                table: "CCC_CTPAT_Questions");

            migrationBuilder.DropIndex(
                name: "IX_CCC_CTPAT_Guidance_IsActive",
                table: "CCC_CTPAT_Guidance");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CCC_CTPAT_Questions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CCC_CTPAT_Questions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CCC_CTPAT_Questions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "CCC_CTPAT_Questions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CCC_CTPAT_Guidance");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CCC_CTPAT_Guidance");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CCC_CTPAT_Guidance");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "CCC_CTPAT_Guidance");

            migrationBuilder.AlterColumn<string>(
                name: "ExternalId",
                table: "CCC_CTPAT_Questions",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);
        }
    }
}
