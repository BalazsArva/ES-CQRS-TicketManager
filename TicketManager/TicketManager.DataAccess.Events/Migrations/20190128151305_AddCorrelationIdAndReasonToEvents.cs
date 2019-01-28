using Microsoft.EntityFrameworkCore.Migrations;

namespace TicketManager.DataAccess.Events.Migrations
{
    public partial class AddCorrelationIdAndReasonToEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "TicketUserInvolvementCancelledEvents",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "TicketUserInvolvementCancelledEvents",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "TicketTypeChangedEvents",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "TicketTypeChangedEvents",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "TicketTitleChangedEvents",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "TicketTitleChangedEvents",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "TicketTagChangedEvents",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "TicketTagChangedEvents",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "TicketStoryPointsChangedEvents",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "TicketStoryPointsChangedEvents",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "TicketStatusChangedEvents",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "TicketStatusChangedEvents",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "TicketPriorityChangedEvents",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "TicketPriorityChangedEvents",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "TicketLinkChangedEvents",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "TicketLinkChangedEvents",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "TicketDescriptionChangedEvents",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "TicketDescriptionChangedEvents",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "TicketCreatedEvents",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "TicketCreatedEvents",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "TicketCommentPostedEvents",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "TicketCommentPostedEvents",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "TicketCommentEditedEvents",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "TicketCommentEditedEvents",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "TicketAssignedEvents",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "TicketAssignedEvents",
                maxLength: 1024,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "TicketUserInvolvementCancelledEvents");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "TicketUserInvolvementCancelledEvents");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "TicketTypeChangedEvents");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "TicketTypeChangedEvents");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "TicketTitleChangedEvents");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "TicketTitleChangedEvents");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "TicketTagChangedEvents");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "TicketTagChangedEvents");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "TicketStoryPointsChangedEvents");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "TicketStoryPointsChangedEvents");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "TicketStatusChangedEvents");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "TicketStatusChangedEvents");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "TicketPriorityChangedEvents");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "TicketPriorityChangedEvents");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "TicketLinkChangedEvents");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "TicketLinkChangedEvents");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "TicketDescriptionChangedEvents");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "TicketDescriptionChangedEvents");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "TicketCreatedEvents");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "TicketCreatedEvents");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "TicketCommentPostedEvents");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "TicketCommentPostedEvents");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "TicketCommentEditedEvents");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "TicketCommentEditedEvents");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "TicketAssignedEvents");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "TicketAssignedEvents");
        }
    }
}
