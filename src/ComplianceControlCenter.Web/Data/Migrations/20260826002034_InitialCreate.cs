using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceControlCenter.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OEA_Activities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Item = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Legal = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Documents = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Owner = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Related = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OEA_Activities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OEA_AuditLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    User = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Changes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OEA_AuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OEA_Roles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OEA_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OEA_Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OEA_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OEA_Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    Author = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AuthorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Text = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OEA_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OEA_Comments_OEA_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "OEA_Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OEA_MonthlyStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CorrectiveActions = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OEA_MonthlyStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OEA_MonthlyStatus_OEA_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "OEA_Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OEA_RoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OEA_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OEA_RoleClaims_OEA_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "OEA_Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OEA_UserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OEA_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OEA_UserClaims_OEA_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "OEA_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OEA_UserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OEA_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_OEA_UserLogins_OEA_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "OEA_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OEA_UserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OEA_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_OEA_UserRoles_OEA_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "OEA_Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OEA_UserRoles_OEA_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "OEA_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OEA_UserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OEA_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_OEA_UserTokens_OEA_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "OEA_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OEA_Activities_IsActive",
                table: "OEA_Activities",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_OEA_Activities_Item",
                table: "OEA_Activities",
                column: "Item");

            migrationBuilder.CreateIndex(
                name: "IX_OEA_AuditLog_EntityName",
                table: "OEA_AuditLog",
                column: "EntityName");

            migrationBuilder.CreateIndex(
                name: "IX_OEA_AuditLog_Timestamp",
                table: "OEA_AuditLog",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_OEA_Comments_ActivityId",
                table: "OEA_Comments",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_OEA_Comments_CreatedAt",
                table: "OEA_Comments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OEA_MonthlyStatus_ActivityId_Year_Month",
                table: "OEA_MonthlyStatus",
                columns: new[] { "ActivityId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OEA_MonthlyStatus_Year_Month",
                table: "OEA_MonthlyStatus",
                columns: new[] { "Year", "Month" });

            migrationBuilder.CreateIndex(
                name: "IX_OEA_RoleClaims_RoleId",
                table: "OEA_RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "OEA_Roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OEA_UserClaims_UserId",
                table: "OEA_UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OEA_UserLogins_UserId",
                table: "OEA_UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OEA_UserRoles_RoleId",
                table: "OEA_UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "OEA_Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "OEA_Users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OEA_AuditLog");

            migrationBuilder.DropTable(
                name: "OEA_Comments");

            migrationBuilder.DropTable(
                name: "OEA_MonthlyStatus");

            migrationBuilder.DropTable(
                name: "OEA_RoleClaims");

            migrationBuilder.DropTable(
                name: "OEA_UserClaims");

            migrationBuilder.DropTable(
                name: "OEA_UserLogins");

            migrationBuilder.DropTable(
                name: "OEA_UserRoles");

            migrationBuilder.DropTable(
                name: "OEA_UserTokens");

            migrationBuilder.DropTable(
                name: "OEA_Activities");

            migrationBuilder.DropTable(
                name: "OEA_Roles");

            migrationBuilder.DropTable(
                name: "OEA_Users");
        }
    }
}
