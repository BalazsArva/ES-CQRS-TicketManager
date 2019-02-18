using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TicketManager.DataAccess.Notifications.Migrations
{
    public partial class InitialModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    User = table.Column<string>(maxLength: 256, nullable: false),
                    SourceSystem = table.Column<string>(maxLength: 256, nullable: false),
                    UtcDateTimeCreated = table.Column<DateTime>(nullable: false),
                    Type = table.Column<string>(maxLength: 256, nullable: false),
                    Title = table.Column<string>(maxLength: 256, nullable: false),
                    BrowserHref = table.Column<string>(nullable: true),
                    ResourceHref = table.Column<string>(nullable: true),
                    IconUri = table.Column<string>(nullable: true),
                    IsRead = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");
        }
    }
}
