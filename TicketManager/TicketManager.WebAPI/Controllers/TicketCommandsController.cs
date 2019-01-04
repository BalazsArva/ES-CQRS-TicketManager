using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TicketManager.Contracts.Common;
using TicketManager.WebAPI.DTOs;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketCommandsController : ControllerBase
    {
        private readonly IMediator mediator;

        public TicketCommandsController(IMediator mediator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            const string user1 = "User 1";
            const string user2 = "User 2";
            const string user3 = "User 3";
            const string user4 = "User 4";
            const string user5 = "User 5";

            var id = await mediator.Send(new CreateTicketCommand(user1, null, "Test ticket", "Test ticket description", new[] { "Infrastructure" }, null, TicketPriorities.Lowest, TicketTypes.Task, TicketStatuses.NotStarted));
            var id2 = await mediator.Send(new CreateTicketCommand(user2, null, "Other test ticket", "Other test ticket description", null, null, TicketPriorities.Medium, TicketTypes.Bug, TicketStatuses.NotStarted));

            await mediator.Send(new AssignTicketCommand(id, user1, user2));
            await mediator.Send(new AssignTicketCommand(id, user1, user3));
            await mediator.Send(new AssignTicketCommand(id, user1, user4));
            await mediator.Send(new AssignTicketCommand(id, user5, user3));

            await mediator.Send(new CancelTicketInvolvementCommand(id, user5, user5));

            await mediator.Send(new EditTicketTitleCommand(id, user1, "Test ticket - edited"));
            await mediator.Send(new EditTicketDescriptionCommand(id, user1, "Test ticket description - edited"));
            await mediator.Send(new ChangeTicketStatusCommand(id, user1, TicketStatuses.InProgress));
            await mediator.Send(new AddTicketTagsCommand(id, user1, new[] { "Dev" }));
            await mediator.Send(new AddTicketTagsCommand(id, user1, new[] { "PoC", "Backend" }));
            await mediator.Send(new RemoveTicketTagsCommand(id, user1, new[] { "Dev" }));
            await mediator.Send(new AddTicketTagsCommand(id, user1, new[] { "QA" }));

            var commentId1 = await mediator.Send(new PostCommentToTicketCommand(id, user1, $"This is a test comment posted to Ticket #{id} by {user1} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}"));
            var commentId2 = await mediator.Send(new PostCommentToTicketCommand(id2, user2, $"This is a test comment posted to Ticket #{id2} by {user2} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}"));

            await mediator.Send(new EditTicketCommentCommand(commentId1, user1, $"This comment was updated by {user1} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}"));

            await mediator.Send(
                new AddTicketLinksCommand(
                    id,
                    user1,
                    new[]
                    {
                        new TicketLinkDTO
                        {
                            LinkType = TicketLinkTypes.PartOf,
                            TargetTicketId = id2
                        },
                        new TicketLinkDTO
                        {
                            LinkType = TicketLinkTypes.RelatedTo,
                            TargetTicketId = id2
                        }
                    }));

            await mediator.Send(
                new RemoveTicketLinksCommand(
                    id,
                    user1,
                    new[]
                    {
                        new TicketLinkDTO
                        {
                            LinkType = TicketLinkTypes.PartOf,
                            TargetTicketId = id2
                        }
                    }));

            await mediator.Send(
                new AddTicketLinksCommand(
                    id,
                    user2,
                    new[]
                    {
                        new TicketLinkDTO
                        {
                            LinkType = TicketLinkTypes.PartOf,
                            TargetTicketId = id2
                        },
                        new TicketLinkDTO
                        {
                            LinkType = TicketLinkTypes.BlockedBy,
                            TargetTicketId = id2
                        }
                    }));

            await mediator.Send(
                new AddTicketLinksCommand(
                    id2,
                    user2,
                    new[]
                    {
                        new TicketLinkDTO
                        {
                            LinkType = TicketLinkTypes.RelatedTo,
                            TargetTicketId = id
                        }
                    }));

            return Ok();
        }
    }
}