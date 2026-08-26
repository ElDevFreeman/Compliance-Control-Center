using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceControlCenter.Web.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameTablesToCccPrefix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OEA_Comments_OEA_Activities_ActivityId",
                table: "OEA_Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_OEA_MonthlyStatus_OEA_Activities_ActivityId",
                table: "OEA_MonthlyStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_OEA_RoleClaims_OEA_Roles_RoleId",
                table: "OEA_RoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_OEA_UserClaims_OEA_Users_UserId",
                table: "OEA_UserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_OEA_UserLogins_OEA_Users_UserId",
                table: "OEA_UserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_OEA_UserRoles_OEA_Roles_RoleId",
                table: "OEA_UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_OEA_UserRoles_OEA_Users_UserId",
                table: "OEA_UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_OEA_UserTokens_OEA_Users_UserId",
                table: "OEA_UserTokens");

            // NOTA (manual): EF quer�a DropTable + CreateTable para AuditLog.
            // Lo reemplazamos por un RenameTable para preservar la historia de auditor�a.
            migrationBuilder.RenameTable(
                name: "OEA_AuditLog",
                newName: "CCC_AuditLog");

            migrationBuilder.RenameIndex(
                name: "IX_OEA_AuditLog_Timestamp",
                table: "CCC_AuditLog",
                newName: "IX_CCC_AuditLog_Timestamp");

            migrationBuilder.RenameIndex(
                name: "IX_OEA_AuditLog_EntityName",
                table: "CCC_AuditLog",
                newName: "IX_CCC_AuditLog_EntityName");

            // PK: el rename de tabla no renombra la constraint PK.
            // Drop & re-add para que el nombre de PK sea consistente.
            migrationBuilder.DropPrimaryKey(
                name: "PK_OEA_AuditLog",
                table: "CCC_AuditLog");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CCC_AuditLog",
                table: "CCC_AuditLog",
                column: "Id");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OEA_UserTokens",
                table: "OEA_UserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OEA_Users",
                table: "OEA_Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OEA_UserRoles",
                table: "OEA_UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OEA_UserLogins",
                table: "OEA_UserLogins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OEA_UserClaims",
                table: "OEA_UserClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OEA_Roles",
                table: "OEA_Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OEA_RoleClaims",
                table: "OEA_RoleClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OEA_MonthlyStatus",
                table: "OEA_MonthlyStatus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OEA_Comments",
                table: "OEA_Comments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OEA_Activities",
                table: "OEA_Activities");

            migrationBuilder.RenameTable(
                name: "OEA_UserTokens",
                newName: "CCC_UserTokens");

            migrationBuilder.RenameTable(
                name: "OEA_Users",
                newName: "CCC_Users");

            migrationBuilder.RenameTable(
                name: "OEA_UserRoles",
                newName: "CCC_UserRoles");

            migrationBuilder.RenameTable(
                name: "OEA_UserLogins",
                newName: "CCC_UserLogins");

            migrationBuilder.RenameTable(
                name: "OEA_UserClaims",
                newName: "CCC_UserClaims");

            migrationBuilder.RenameTable(
                name: "OEA_Roles",
                newName: "CCC_Roles");

            migrationBuilder.RenameTable(
                name: "OEA_RoleClaims",
                newName: "CCC_RoleClaims");

            migrationBuilder.RenameTable(
                name: "OEA_MonthlyStatus",
                newName: "CCC_OEA_MonthlyStatus");

            migrationBuilder.RenameTable(
                name: "OEA_Comments",
                newName: "CCC_OEA_Comments");

            migrationBuilder.RenameTable(
                name: "OEA_Activities",
                newName: "CCC_OEA_Activities");

            migrationBuilder.RenameIndex(
                name: "IX_OEA_UserRoles_RoleId",
                table: "CCC_UserRoles",
                newName: "IX_CCC_UserRoles_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_OEA_UserLogins_UserId",
                table: "CCC_UserLogins",
                newName: "IX_CCC_UserLogins_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_OEA_UserClaims_UserId",
                table: "CCC_UserClaims",
                newName: "IX_CCC_UserClaims_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_OEA_RoleClaims_RoleId",
                table: "CCC_RoleClaims",
                newName: "IX_CCC_RoleClaims_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_OEA_MonthlyStatus_Year_Month",
                table: "CCC_OEA_MonthlyStatus",
                newName: "IX_CCC_OEA_MonthlyStatus_Year_Month");

            migrationBuilder.RenameIndex(
                name: "IX_OEA_MonthlyStatus_ActivityId_Year_Month",
                table: "CCC_OEA_MonthlyStatus",
                newName: "IX_CCC_OEA_MonthlyStatus_ActivityId_Year_Month");

            migrationBuilder.RenameIndex(
                name: "IX_OEA_Comments_CreatedAt",
                table: "CCC_OEA_Comments",
                newName: "IX_CCC_OEA_Comments_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_OEA_Comments_ActivityId",
                table: "CCC_OEA_Comments",
                newName: "IX_CCC_OEA_Comments_ActivityId");

            migrationBuilder.RenameIndex(
                name: "IX_OEA_Activities_Item",
                table: "CCC_OEA_Activities",
                newName: "IX_CCC_OEA_Activities_Item");

            migrationBuilder.RenameIndex(
                name: "IX_OEA_Activities_IsActive",
                table: "CCC_OEA_Activities",
                newName: "IX_CCC_OEA_Activities_IsActive");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CCC_UserTokens",
                table: "CCC_UserTokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CCC_Users",
                table: "CCC_Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CCC_UserRoles",
                table: "CCC_UserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CCC_UserLogins",
                table: "CCC_UserLogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CCC_UserClaims",
                table: "CCC_UserClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CCC_Roles",
                table: "CCC_Roles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CCC_RoleClaims",
                table: "CCC_RoleClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CCC_OEA_MonthlyStatus",
                table: "CCC_OEA_MonthlyStatus",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CCC_OEA_Comments",
                table: "CCC_OEA_Comments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CCC_OEA_Activities",
                table: "CCC_OEA_Activities",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CCC_OEA_Comments_CCC_OEA_Activities_ActivityId",
                table: "CCC_OEA_Comments",
                column: "ActivityId",
                principalTable: "CCC_OEA_Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CCC_OEA_MonthlyStatus_CCC_OEA_Activities_ActivityId",
                table: "CCC_OEA_MonthlyStatus",
                column: "ActivityId",
                principalTable: "CCC_OEA_Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CCC_RoleClaims_CCC_Roles_RoleId",
                table: "CCC_RoleClaims",
                column: "RoleId",
                principalTable: "CCC_Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CCC_UserClaims_CCC_Users_UserId",
                table: "CCC_UserClaims",
                column: "UserId",
                principalTable: "CCC_Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CCC_UserLogins_CCC_Users_UserId",
                table: "CCC_UserLogins",
                column: "UserId",
                principalTable: "CCC_Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CCC_UserRoles_CCC_Roles_RoleId",
                table: "CCC_UserRoles",
                column: "RoleId",
                principalTable: "CCC_Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CCC_UserRoles_CCC_Users_UserId",
                table: "CCC_UserRoles",
                column: "UserId",
                principalTable: "CCC_Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CCC_UserTokens_CCC_Users_UserId",
                table: "CCC_UserTokens",
                column: "UserId",
                principalTable: "CCC_Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CCC_OEA_Comments_CCC_OEA_Activities_ActivityId",
                table: "CCC_OEA_Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_CCC_OEA_MonthlyStatus_CCC_OEA_Activities_ActivityId",
                table: "CCC_OEA_MonthlyStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_CCC_RoleClaims_CCC_Roles_RoleId",
                table: "CCC_RoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_CCC_UserClaims_CCC_Users_UserId",
                table: "CCC_UserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_CCC_UserLogins_CCC_Users_UserId",
                table: "CCC_UserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_CCC_UserRoles_CCC_Roles_RoleId",
                table: "CCC_UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_CCC_UserRoles_CCC_Users_UserId",
                table: "CCC_UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_CCC_UserTokens_CCC_Users_UserId",
                table: "CCC_UserTokens");

            // Manual: revertir el RenameTable de AuditLog
            migrationBuilder.DropPrimaryKey(
                name: "PK_CCC_AuditLog",
                table: "CCC_AuditLog");

            migrationBuilder.RenameIndex(
                name: "IX_CCC_AuditLog_EntityName",
                table: "CCC_AuditLog",
                newName: "IX_OEA_AuditLog_EntityName");

            migrationBuilder.RenameIndex(
                name: "IX_CCC_AuditLog_Timestamp",
                table: "CCC_AuditLog",
                newName: "IX_OEA_AuditLog_Timestamp");

            migrationBuilder.RenameTable(
                name: "CCC_AuditLog",
                newName: "OEA_AuditLog");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OEA_AuditLog",
                table: "OEA_AuditLog",
                column: "Id");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CCC_UserTokens",
                table: "CCC_UserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CCC_Users",
                table: "CCC_Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CCC_UserRoles",
                table: "CCC_UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CCC_UserLogins",
                table: "CCC_UserLogins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CCC_UserClaims",
                table: "CCC_UserClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CCC_Roles",
                table: "CCC_Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CCC_RoleClaims",
                table: "CCC_RoleClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CCC_OEA_MonthlyStatus",
                table: "CCC_OEA_MonthlyStatus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CCC_OEA_Comments",
                table: "CCC_OEA_Comments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CCC_OEA_Activities",
                table: "CCC_OEA_Activities");

            migrationBuilder.RenameTable(
                name: "CCC_UserTokens",
                newName: "OEA_UserTokens");

            migrationBuilder.RenameTable(
                name: "CCC_Users",
                newName: "OEA_Users");

            migrationBuilder.RenameTable(
                name: "CCC_UserRoles",
                newName: "OEA_UserRoles");

            migrationBuilder.RenameTable(
                name: "CCC_UserLogins",
                newName: "OEA_UserLogins");

            migrationBuilder.RenameTable(
                name: "CCC_UserClaims",
                newName: "OEA_UserClaims");

            migrationBuilder.RenameTable(
                name: "CCC_Roles",
                newName: "OEA_Roles");

            migrationBuilder.RenameTable(
                name: "CCC_RoleClaims",
                newName: "OEA_RoleClaims");

            migrationBuilder.RenameTable(
                name: "CCC_OEA_MonthlyStatus",
                newName: "OEA_MonthlyStatus");

            migrationBuilder.RenameTable(
                name: "CCC_OEA_Comments",
                newName: "OEA_Comments");

            migrationBuilder.RenameTable(
                name: "CCC_OEA_Activities",
                newName: "OEA_Activities");

            migrationBuilder.RenameIndex(
                name: "IX_CCC_UserRoles_RoleId",
                table: "OEA_UserRoles",
                newName: "IX_OEA_UserRoles_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_CCC_UserLogins_UserId",
                table: "OEA_UserLogins",
                newName: "IX_OEA_UserLogins_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CCC_UserClaims_UserId",
                table: "OEA_UserClaims",
                newName: "IX_OEA_UserClaims_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CCC_RoleClaims_RoleId",
                table: "OEA_RoleClaims",
                newName: "IX_OEA_RoleClaims_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_CCC_OEA_MonthlyStatus_Year_Month",
                table: "OEA_MonthlyStatus",
                newName: "IX_OEA_MonthlyStatus_Year_Month");

            migrationBuilder.RenameIndex(
                name: "IX_CCC_OEA_MonthlyStatus_ActivityId_Year_Month",
                table: "OEA_MonthlyStatus",
                newName: "IX_OEA_MonthlyStatus_ActivityId_Year_Month");

            migrationBuilder.RenameIndex(
                name: "IX_CCC_OEA_Comments_CreatedAt",
                table: "OEA_Comments",
                newName: "IX_OEA_Comments_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_CCC_OEA_Comments_ActivityId",
                table: "OEA_Comments",
                newName: "IX_OEA_Comments_ActivityId");

            migrationBuilder.RenameIndex(
                name: "IX_CCC_OEA_Activities_Item",
                table: "OEA_Activities",
                newName: "IX_OEA_Activities_Item");

            migrationBuilder.RenameIndex(
                name: "IX_CCC_OEA_Activities_IsActive",
                table: "OEA_Activities",
                newName: "IX_OEA_Activities_IsActive");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OEA_UserTokens",
                table: "OEA_UserTokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_OEA_Users",
                table: "OEA_Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OEA_UserRoles",
                table: "OEA_UserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_OEA_UserLogins",
                table: "OEA_UserLogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_OEA_UserClaims",
                table: "OEA_UserClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OEA_Roles",
                table: "OEA_Roles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OEA_RoleClaims",
                table: "OEA_RoleClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OEA_MonthlyStatus",
                table: "OEA_MonthlyStatus",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OEA_Comments",
                table: "OEA_Comments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OEA_Activities",
                table: "OEA_Activities",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OEA_Comments_OEA_Activities_ActivityId",
                table: "OEA_Comments",
                column: "ActivityId",
                principalTable: "OEA_Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OEA_MonthlyStatus_OEA_Activities_ActivityId",
                table: "OEA_MonthlyStatus",
                column: "ActivityId",
                principalTable: "OEA_Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OEA_RoleClaims_OEA_Roles_RoleId",
                table: "OEA_RoleClaims",
                column: "RoleId",
                principalTable: "OEA_Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OEA_UserClaims_OEA_Users_UserId",
                table: "OEA_UserClaims",
                column: "UserId",
                principalTable: "OEA_Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OEA_UserLogins_OEA_Users_UserId",
                table: "OEA_UserLogins",
                column: "UserId",
                principalTable: "OEA_Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OEA_UserRoles_OEA_Roles_RoleId",
                table: "OEA_UserRoles",
                column: "RoleId",
                principalTable: "OEA_Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OEA_UserRoles_OEA_Users_UserId",
                table: "OEA_UserRoles",
                column: "UserId",
                principalTable: "OEA_Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OEA_UserTokens_OEA_Users_UserId",
                table: "OEA_UserTokens",
                column: "UserId",
                principalTable: "OEA_Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
