using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TicketManager.DataAccess.Events.Migrations
{
    public partial class AddingStoryPointsChangedEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketStoryPointsChangedEvents",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false),
                    TicketCreatedEventId = table.Column<long>(nullable: false),
                    StoryPoints = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketStoryPointsChangedEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketStoryPointsChangedEvents_TicketCreatedEvents_TicketCreatedEventId",
                        column: x => x.TicketCreatedEventId,
                        principalTable: "TicketCreatedEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketStoryPointsChangedEvents_TicketCreatedEventId",
                table: "TicketStoryPointsChangedEvents",
                column: "TicketCreatedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketStoryPointsChangedEvents_UtcDateRecorded",
                table: "TicketStoryPointsChangedEvents",
                column: "UtcDateRecorded");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketStoryPointsChangedEvents");
        }
    }
}
