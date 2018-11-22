using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TicketManager.DataAccess.Events.Migrations
{
    public partial class IndexesAndAddingTicketAssignedEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketAssignedEvents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false),
                    TicketCreatedEventId = table.Column<int>(nullable: false),
                    AssignedTo = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketAssignedEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketAssignedEvents_TicketCreatedEvents_TicketCreatedEventId",
                        column: x => x.TicketCreatedEventId,
                        principalTable: "TicketCreatedEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketTagChangedEvents_UtcDateRecorded",
                table: "TicketTagChangedEvents",
                column: "UtcDateRecorded");

            migrationBuilder.CreateIndex(
                name: "IX_TicketStatusChangedEvents_UtcDateRecorded",
                table: "TicketStatusChangedEvents",
                column: "UtcDateRecorded");

            migrationBuilder.CreateIndex(
                name: "IX_TicketLinkChangedEvents_UtcDateRecorded",
                table: "TicketLinkChangedEvents",
                column: "UtcDateRecorded");

            migrationBuilder.CreateIndex(
                name: "IX_TicketDetailsChangedEvents_UtcDateRecorded",
                table: "TicketDetailsChangedEvents",
                column: "UtcDateRecorded");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCreatedEvents_UtcDateRecorded",
                table: "TicketCreatedEvents",
                column: "UtcDateRecorded");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCommentPostedEvents_UtcDateRecorded",
                table: "TicketCommentPostedEvents",
                column: "UtcDateRecorded");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCommentEditedEvents_UtcDateRecorded",
                table: "TicketCommentEditedEvents",
                column: "UtcDateRecorded");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAssignedEvents_TicketCreatedEventId",
                table: "TicketAssignedEvents",
                column: "TicketCreatedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAssignedEvents_UtcDateRecorded",
                table: "TicketAssignedEvents",
                column: "UtcDateRecorded");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketAssignedEvents");

            migrationBuilder.DropIndex(
                name: "IX_TicketTagChangedEvents_UtcDateRecorded",
                table: "TicketTagChangedEvents");

            migrationBuilder.DropIndex(
                name: "IX_TicketStatusChangedEvents_UtcDateRecorded",
                table: "TicketStatusChangedEvents");

            migrationBuilder.DropIndex(
                name: "IX_TicketLinkChangedEvents_UtcDateRecorded",
                table: "TicketLinkChangedEvents");

            migrationBuilder.DropIndex(
                name: "IX_TicketDetailsChangedEvents_UtcDateRecorded",
                table: "TicketDetailsChangedEvents");

            migrationBuilder.DropIndex(
                name: "IX_TicketCreatedEvents_UtcDateRecorded",
                table: "TicketCreatedEvents");

            migrationBuilder.DropIndex(
                name: "IX_TicketCommentPostedEvents_UtcDateRecorded",
                table: "TicketCommentPostedEvents");

            migrationBuilder.DropIndex(
                name: "IX_TicketCommentEditedEvents_UtcDateRecorded",
                table: "TicketCommentEditedEvents");
        }
    }
}
