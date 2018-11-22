using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TicketManager.DataAccess.Events.Migrations
{
    public partial class InitialModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketCreatedEvents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketCreatedEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketCommentPostedEvents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false),
                    TicketCreatedEventId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketCommentPostedEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketCommentPostedEvents_TicketCreatedEvents_TicketCreatedEventId",
                        column: x => x.TicketCreatedEventId,
                        principalTable: "TicketCreatedEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketDetailsChangedEvents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false),
                    TicketCreatedEventId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(maxLength: 256, nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Priority = table.Column<int>(nullable: false),
                    TicketType = table.Column<int>(nullable: false)
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

            migrationBuilder.CreateTable(
                name: "TicketLinkChangedEvents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false),
                    SourceTicketCreatedEventId = table.Column<int>(nullable: false),
                    TargetTicketCreatedEventId = table.Column<int>(nullable: false),
                    LinkType = table.Column<int>(nullable: false),
                    ConnectionIsActive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketLinkChangedEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketLinkChangedEvents_TicketCreatedEvents_SourceTicketCreatedEventId",
                        column: x => x.SourceTicketCreatedEventId,
                        principalTable: "TicketCreatedEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketLinkChangedEvents_TicketCreatedEvents_TargetTicketCreatedEventId",
                        column: x => x.TargetTicketCreatedEventId,
                        principalTable: "TicketCreatedEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TicketStatusChangedEvents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false),
                    TicketCreatedEventId = table.Column<int>(nullable: false),
                    TicketStatus = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketStatusChangedEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketStatusChangedEvents_TicketCreatedEvents_TicketCreatedEventId",
                        column: x => x.TicketCreatedEventId,
                        principalTable: "TicketCreatedEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketTagChangedEvents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false),
                    TicketCreatedEventId = table.Column<int>(nullable: false),
                    Tag = table.Column<string>(maxLength: 64, nullable: false),
                    TagAdded = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketTagChangedEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketTagChangedEvents_TicketCreatedEvents_TicketCreatedEventId",
                        column: x => x.TicketCreatedEventId,
                        principalTable: "TicketCreatedEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketCommentEditedEvents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false),
                    TicketCommentPostedEventId = table.Column<int>(nullable: false),
                    CommentText = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketCommentEditedEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketCommentEditedEvents_TicketCommentPostedEvents_TicketCommentPostedEventId",
                        column: x => x.TicketCommentPostedEventId,
                        principalTable: "TicketCommentPostedEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketCommentEditedEvents_TicketCommentPostedEventId",
                table: "TicketCommentEditedEvents",
                column: "TicketCommentPostedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCommentPostedEvents_TicketCreatedEventId",
                table: "TicketCommentPostedEvents",
                column: "TicketCreatedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketDetailsChangedEvents_TicketCreatedEventId",
                table: "TicketDetailsChangedEvents",
                column: "TicketCreatedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketLinkChangedEvents_SourceTicketCreatedEventId",
                table: "TicketLinkChangedEvents",
                column: "SourceTicketCreatedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketLinkChangedEvents_TargetTicketCreatedEventId",
                table: "TicketLinkChangedEvents",
                column: "TargetTicketCreatedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketStatusChangedEvents_TicketCreatedEventId",
                table: "TicketStatusChangedEvents",
                column: "TicketCreatedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTagChangedEvents_TicketCreatedEventId",
                table: "TicketTagChangedEvents",
                column: "TicketCreatedEventId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketCommentEditedEvents");

            migrationBuilder.DropTable(
                name: "TicketDetailsChangedEvents");

            migrationBuilder.DropTable(
                name: "TicketLinkChangedEvents");

            migrationBuilder.DropTable(
                name: "TicketStatusChangedEvents");

            migrationBuilder.DropTable(
                name: "TicketTagChangedEvents");

            migrationBuilder.DropTable(
                name: "TicketCommentPostedEvents");

            migrationBuilder.DropTable(
                name: "TicketCreatedEvents");
        }
    }
}
