using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class AddTradingJournalUserRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TradingStrategies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradingStrategies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TradingStrategies_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActiveTrades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Instrument = table.Column<string>(type: "text", nullable: false),
                    StrategyId = table.Column<int>(type: "integer", nullable: false),
                    PositionType = table.Column<int>(type: "integer", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EntryPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    PositionSize = table.Column<decimal>(type: "numeric", nullable: false),
                    ExitDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExitPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    TotalFees = table.Column<decimal>(type: "numeric", nullable: false),
                    RealizedPnL = table.Column<decimal>(type: "numeric", nullable: true),
                    ChartImageUrl = table.Column<string>(type: "text", nullable: true),
                    ReviewNotes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveTrades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActiveTrades_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActiveTrades_TradingStrategies_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "TradingStrategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActiveTrades_StrategyId",
                table: "ActiveTrades",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_ActiveTrades_UserId",
                table: "ActiveTrades",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TradingStrategies_UserId",
                table: "TradingStrategies",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveTrades");

            migrationBuilder.DropTable(
                name: "TradingStrategies");
        }
    }
}
