using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceControlCenter.Web.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCtpatModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CCC_CTPAT_Guidance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Criterio = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    RespTip = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Revisar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Evidencia = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CCC_CTPAT_Guidance", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CCC_CTPAT_Questions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Criterio = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Pregunta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Respuesta2025 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CCC_CTPAT_Questions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CCC_CTPAT_Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    EvidenciaRevisada = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CambiosDetectados = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RespuestaNueva = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaRevision = table.Column<DateOnly>(type: "date", nullable: true),
                    Revisor = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Comentarios = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CCC_CTPAT_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CCC_CTPAT_Reviews_CCC_CTPAT_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "CCC_CTPAT_Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CCC_CTPAT_ReviewFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReviewId = table.Column<int>(type: "int", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    RelativePath = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CCC_CTPAT_ReviewFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CCC_CTPAT_ReviewFiles_CCC_CTPAT_Reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "CCC_CTPAT_Reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CCC_CTPAT_Guidance_Criterio",
                table: "CCC_CTPAT_Guidance",
                column: "Criterio");

            migrationBuilder.CreateIndex(
                name: "IX_CCC_CTPAT_Guidance_GroupName",
                table: "CCC_CTPAT_Guidance",
                column: "GroupName");

            migrationBuilder.CreateIndex(
                name: "IX_CCC_CTPAT_Questions_Criterio",
                table: "CCC_CTPAT_Questions",
                column: "Criterio");

            migrationBuilder.CreateIndex(
                name: "IX_CCC_CTPAT_Questions_ExternalId",
                table: "CCC_CTPAT_Questions",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CCC_CTPAT_Questions_SortOrder",
                table: "CCC_CTPAT_Questions",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_CCC_CTPAT_ReviewFiles_ExternalId",
                table: "CCC_CTPAT_ReviewFiles",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CCC_CTPAT_ReviewFiles_ReviewId",
                table: "CCC_CTPAT_ReviewFiles",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_CCC_CTPAT_Reviews_QuestionId_Year",
                table: "CCC_CTPAT_Reviews",
                columns: new[] { "QuestionId", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CCC_CTPAT_Reviews_Status",
                table: "CCC_CTPAT_Reviews",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CCC_CTPAT_Guidance");

            migrationBuilder.DropTable(
                name: "CCC_CTPAT_ReviewFiles");

            migrationBuilder.DropTable(
                name: "CCC_CTPAT_Reviews");

            migrationBuilder.DropTable(
                name: "CCC_CTPAT_Questions");
        }
    }
}
