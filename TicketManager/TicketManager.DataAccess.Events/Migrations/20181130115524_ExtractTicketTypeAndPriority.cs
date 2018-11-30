using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TicketManager.DataAccess.Events.Migrations
{
    public partial class ExtractTicketTypeAndPriority : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "TicketDetailsChangedEvents");

            migrationBuilder.DropColumn(
                name: "TicketType",
                table: "TicketDetailsChangedEvents");

            migrationBuilder.CreateTable(
                name: "TicketPriorityChangedEvents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false),
                    TicketCreatedEventId = table.Column<int>(nullable: false),
                    Priority = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketPriorityChangedEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketPriorityChangedEvents_TicketCreatedEvents_TicketCreatedEventId",
                        column: x => x.TicketCreatedEventId,
                        principalTable: "TicketCreatedEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketTypeChangedEvents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false),
                    TicketCreatedEventId = table.Column<int>(nullable: false),
                    TicketType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketTypeChangedEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketTypeChangedEvents_TicketCreatedEvents_TicketCreatedEventId",
                        column: x => x.TicketCreatedEventId,
                        principalTable: "TicketCreatedEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketPriorityChangedEvents_TicketCreatedEventId",
                table: "TicketPriorityChangedEvents",
                column: "TicketCreatedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketPriorityChangedEvents_UtcDateRecorded",
                table: "TicketPriorityChangedEvents",
                column: "UtcDateRecorded");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTypeChangedEvents_TicketCreatedEventId",
                table: "TicketTypeChangedEvents",
                column: "TicketCreatedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTypeChangedEvents_UtcDateRecorded",
                table: "TicketTypeChangedEvents",
                column: "UtcDateRecorded");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketPriorityChangedEvents");

            migrationBuilder.DropTable(
                name: "TicketTypeChangedEvents");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "TicketDetailsChangedEvents",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TicketType",
                table: "TicketDetailsChangedEvents",
                nullable: false,
                defaultValue: 0);
        }
    }
}
