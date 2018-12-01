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
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketCreatedEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketAssignedEvents",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false),
                    TicketCreatedEventId = table.Column<long>(nullable: false),
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

            migrationBuilder.CreateTable(
                name: "TicketCommentPostedEvents",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false),
                    TicketCreatedEventId = table.Column<long>(nullable: false)
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
                name: "TicketDescriptionChangedEvents",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false),
                    TicketCreatedEventId = table.Column<long>(nullable: false),
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
                name: "TicketLinkChangedEvents",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false),
                    SourceTicketCreatedEventId = table.Column<long>(nullable: false),
                    TargetTicketCreatedEventId = table.Column<long>(nullable: false),
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
                name: "TicketPriorityChangedEvents",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false),
                    TicketCreatedEventId = table.Column<long>(nullable: false),
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
                name: "TicketStatusChangedEvents",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false),
                    TicketCreatedEventId = table.Column<long>(nullable: false),
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
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false),
                    TicketCreatedEventId = table.Column<long>(nullable: false),
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
                name: "TicketTitleChangedEvents",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false),
                    TicketCreatedEventId = table.Column<long>(nullable: false),
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

            migrationBuilder.CreateTable(
                name: "TicketTypeChangedEvents",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false),
                    TicketCreatedEventId = table.Column<long>(nullable: false),
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

            migrationBuilder.CreateTable(
                name: "TicketCommentEditedEvents",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UtcDateRecorded = table.Column<DateTime>(nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CausedBy = table.Column<string>(maxLength: 256, nullable: false),
                    TicketCommentPostedEventId = table.Column<long>(nullable: false),
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
                name: "IX_TicketAssignedEvents_TicketCreatedEventId",
                table: "TicketAssignedEvents",
                column: "TicketCreatedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAssignedEvents_UtcDateRecorded",
                table: "TicketAssignedEvents",
                column: "UtcDateRecorded");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCommentEditedEvents_TicketCommentPostedEventId",
                table: "TicketCommentEditedEvents",
                column: "TicketCommentPostedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCommentEditedEvents_UtcDateRecorded",
                table: "TicketCommentEditedEvents",
                column: "UtcDateRecorded");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCommentPostedEvents_TicketCreatedEventId",
                table: "TicketCommentPostedEvents",
                column: "TicketCreatedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCommentPostedEvents_UtcDateRecorded",
                table: "TicketCommentPostedEvents",
                column: "UtcDateRecorded");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCreatedEvents_UtcDateRecorded",
                table: "TicketCreatedEvents",
                column: "UtcDateRecorded");

            migrationBuilder.CreateIndex(
                name: "IX_TicketDescriptionChangedEvents_TicketCreatedEventId",
                table: "TicketDescriptionChangedEvents",
                column: "TicketCreatedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketDescriptionChangedEvents_UtcDateRecorded",
                table: "TicketDescriptionChangedEvents",
                column: "UtcDateRecorded");

            migrationBuilder.CreateIndex(
                name: "IX_TicketLinkChangedEvents_SourceTicketCreatedEventId",
                table: "TicketLinkChangedEvents",
                column: "SourceTicketCreatedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketLinkChangedEvents_TargetTicketCreatedEventId",
                table: "TicketLinkChangedEvents",
                column: "TargetTicketCreatedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketLinkChangedEvents_UtcDateRecorded",
                table: "TicketLinkChangedEvents",
                column: "UtcDateRecorded");

            migrationBuilder.CreateIndex(
                name: "IX_TicketPriorityChangedEvents_TicketCreatedEventId",
                table: "TicketPriorityChangedEvents",
                column: "TicketCreatedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketPriorityChangedEvents_UtcDateRecorded",
                table: "TicketPriorityChangedEvents",
                column: "UtcDateRecorded");

            migrationBuilder.CreateIndex(
                name: "IX_TicketStatusChangedEvents_TicketCreatedEventId",
                table: "TicketStatusChangedEvents",
                column: "TicketCreatedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketStatusChangedEvents_UtcDateRecorded",
                table: "TicketStatusChangedEvents",
                column: "UtcDateRecorded");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTagChangedEvents_TicketCreatedEventId",
                table: "TicketTagChangedEvents",
                column: "TicketCreatedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTagChangedEvents_UtcDateRecorded",
                table: "TicketTagChangedEvents",
                column: "UtcDateRecorded");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTitleChangedEvents_TicketCreatedEventId",
                table: "TicketTitleChangedEvents",
                column: "TicketCreatedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTitleChangedEvents_UtcDateRecorded",
                table: "TicketTitleChangedEvents",
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
                name: "TicketAssignedEvents");

            migrationBuilder.DropTable(
                name: "TicketCommentEditedEvents");

            migrationBuilder.DropTable(
                name: "TicketDescriptionChangedEvents");

            migrationBuilder.DropTable(
                name: "TicketLinkChangedEvents");

            migrationBuilder.DropTable(
                name: "TicketPriorityChangedEvents");

            migrationBuilder.DropTable(
                name: "TicketStatusChangedEvents");

            migrationBuilder.DropTable(
                name: "TicketTagChangedEvents");

            migrationBuilder.DropTable(
                name: "TicketTitleChangedEvents");

            migrationBuilder.DropTable(
                name: "TicketTypeChangedEvents");

            migrationBuilder.DropTable(
                name: "TicketCommentPostedEvents");

            migrationBuilder.DropTable(
                name: "TicketCreatedEvents");
        }
    }
}
