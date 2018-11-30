using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TicketManager.DataAccess.Events.Migrations
{
    public partial class ExtractTicketTitleAndDescription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketDetailsChangedEvents");

            migrationBuilder.CreateTable(
                name: "TicketDescriptionChangedEvents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false),
                    TicketCreatedEventId = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketDescriptionChangedEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketDescriptionChangedEvents_TicketCreatedEvents_TicketCreatedEventId",
                        column: x => x.TicketCreatedEventId,
                        principalTable: "TicketCreatedEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketTitleChangedEvents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false),
                    TicketCreatedEventId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketTitleChangedEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketTitleChangedEvents_TicketCreatedEvents_TicketCreatedEventId",
                        column: x => x.TicketCreatedEventId,
                        principalTable: "TicketCreatedEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketDescriptionChangedEvents_TicketCreatedEventId",
                table: "TicketDescriptionChangedEvents",
                column: "TicketCreatedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketDescriptionChangedEvents_UtcDateRecorded",
                table: "TicketDescriptionChangedEvents",
                column: "UtcDateRecorded");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTitleChangedEvents_TicketCreatedEventId",
                table: "TicketTitleChangedEvents",
                column: "TicketCreatedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTitleChangedEvents_UtcDateRecorded",
                table: "TicketTitleChangedEvents",
                column: "UtcDateRecorded");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketDescriptionChangedEvents");

            migrationBuilder.DropTable(
                name: "TicketTitleChangedEvents");

            migrationBuilder.CreateTable(
                name: "TicketDetailsChangedEvents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false),
                    Description = table.Column<string>(nullable: true),
                    TicketCreatedEventId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(maxLength: 256, nullable: false),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketDetailsChangedEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketDetailsChangedEvents_TicketCreatedEvents_TicketCreatedEventId",
                        column: x => x.TicketCreatedEventId,
                        principalTable: "TicketCreatedEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketDetailsChangedEvents_TicketCreatedEventId",
                table: "TicketDetailsChangedEvents",
                column: "TicketCreatedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketDetailsChangedEvents_UtcDateRecorded",
                table: "TicketDetailsChangedEvents",
                column: "UtcDateRecorded");
        }
    }
}
