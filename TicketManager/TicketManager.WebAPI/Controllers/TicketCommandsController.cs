﻿using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicketManager.Contracts.CommandApi;
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

            var id = await mediator.Send(new CreateTicketCommand(user1, null, "Test ticket", "Test ticket description", new[] { "Infrastructure" }, 5, null, TicketPriorities.Lowest, TicketTypes.Task, TicketStatuses.NotStarted));
            var id2 = await mediator.Send(new CreateTicketCommand(user2, null, "Other test ticket", "Other test ticket description", null, 2, null, TicketPriorities.Medium, TicketTypes.Bug, TicketStatuses.NotStarted));

            await mediator.Send(new AssignTicketCommand(id, user1, user2));
            await mediator.Send(new AssignTicketCommand(id, user1, user3));
            await mediator.Send(new AssignTicketCommand(id, user1, user4));
            await mediator.Send(new AssignTicketCommand(id, user5, user3));

            await mediator.Send(new CancelTicketInvolvementCommand(id, user5, user5));

            await mediator.Send(new ChangeTicketTitleCommand(id, user1, "Test ticket - edited"));
            await mediator.Send(new ChangeTicketDescriptionCommand(id, user1, "Test ticket description - edited"));
            await mediator.Send(new ChangeTicketStatusCommand(id, user1, TicketStatuses.InProgress));
            await mediator.Send(new AddTicketTagsCommand(id, user1, new[] { "Dev" }));
            await mediator.Send(new AddTicketTagsCommand(id, user1, new[] { "PoC", "Backend" }));
            await mediator.Send(new RemoveTicketTagsCommand(id, user1, new[] { "Dev" }));
            await mediator.Send(new AddTicketTagsCommand(id, user1, new[] { "QA" }));
            await mediator.Send(new ChangeTicketStoryPointsCommand(id, user2, 10));
            await mediator.Send(new ChangeTicketStoryPointsCommand(id, user2, 3));
            await mediator.Send(new ChangeTicketStoryPointsCommand(id2, user1, 4));

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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> CreateTicket([FromBody]CreateTicketCommand command)
        {
            var id = await mediator.Send(command);

            return CreatedAtRoute(RouteNames.Tickets_Queries_Get_ById_Extended, new { id }, value: null);
        }

        [HttpPatch]
        [Route("{id:int}/assignment")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> AssignTicket([FromRoute]long id, [FromBody]AssignTicketCommandModel commandModel)
        {
            var command = new AssignTicketCommand(id, commandModel.RaisedByUser, commandModel.RaisedByUser);

            await mediator.Send(command);

            return Accepted();
        }

        [HttpPatch]
        [Route("{id:int}/storyPoints")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> ChangeTicketStoryPoints([FromRoute]long id, [FromBody]ChangeTicketStoryPointsCommandModel commandModel)
        {
            var command = new ChangeTicketStoryPointsCommand(id, commandModel.RaisedByUser, commandModel.StoryPoints);

            await mediator.Send(command);

            return Accepted();
        }

        [HttpPatch]
        [Route("{id:int}/type")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> ChangeTicketType([FromRoute]long id, [FromBody]ChangeTicketTypeCommandModel commandModel)
        {
            var command = new ChangeTicketTypeCommand(id, commandModel.RaisedByUser, commandModel.TicketType);

            await mediator.Send(command);

            return Accepted();
        }

        [HttpPatch]
        [Route("{id:int}/priority")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> ChangeTicketPriority([FromRoute]long id, [FromBody]ChangeTicketPriorityCommandModel commandModel)
        {
            var command = new ChangeTicketPriorityCommand(id, commandModel.RaisedByUser, commandModel.Priority);

            await mediator.Send(command);

            return Accepted();
        }

        [HttpPatch]
        [Route("{id:int}/status")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> ChangeTicketStatus([FromRoute]long id, [FromBody]ChangeTicketStatusCommandModel commandModel)
        {
            var command = new ChangeTicketStatusCommand(id, commandModel.RaisedByUser, commandModel.Status);

            await mediator.Send(command);

            return Accepted();
        }

        [HttpPatch]
        [Route("{id:int}/title")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> ChangeTicketTitle([FromRoute]long id, [FromBody]ChangeTicketTitleCommandModel commandModel)
        {
            var command = new ChangeTicketTitleCommand(id, commandModel.RaisedByUser, commandModel.Title);

            await mediator.Send(command);

            return Accepted();
        }

        [HttpPatch]
        [Route("{id:int}/description")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> ChangeTicketDescription([FromRoute]long id, [FromBody]ChangeTicketDescriptionCommandModel commandModel)
        {
            var command = new ChangeTicketDescriptionCommand(id, commandModel.RaisedByUser, commandModel.Description);

            await mediator.Send(command);

            return Accepted();
        }

        [HttpDelete]
        [Route("{id:int}/involvement/{user}")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> CancelTicketUserInvolvement([FromRoute]long id, string user)
        {
            var command = new CancelTicketInvolvementCommand(id, user, user);

            await mediator.Send(command);

            return Accepted();
        }

        [HttpPost]
        [Route("{id:int}/tags")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> AddTags([FromRoute]long id, TicketTagsCommandModel commandModel)
        {
            var command = new AddTicketTagsCommand(id, commandModel.RaisedByUser, commandModel.Tags);

            await mediator.Send(command);

            return Accepted();
        }

        [HttpDelete]
        [Route("{id:int}/tags")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> RemoveTags([FromRoute]long id, TicketTagsCommandModel commandModel)
        {
            var command = new RemoveTicketTagsCommand(id, commandModel.RaisedByUser, commandModel.Tags);

            await mediator.Send(command);

            return Accepted();
        }
    }
}