using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
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
            var id = await mediator.Send(new CreateTicketCommand
            {
                RaisedByUser = "Balazs",
                Description = "Test",
                Priority = Domain.Common.TicketPriorities.Lowest,
                TicketType = Domain.Common.TicketTypes.Task,
                Title = "Test"
            });

            var id2 = await mediator.Send(new CreateTicketCommand
            {
                RaisedByUser = "Balazs2",
                Description = "Test2",
                Priority = Domain.Common.TicketPriorities.Lowest,
                TicketType = Domain.Common.TicketTypes.Task,
                Title = "Test2"
            });

            //id = 26023;

            await mediator.Send(new AssignTicketCommand
            {
                TicketId = id,
                RaisedByUser = "Balazs2",
                AssignTo = "Balazs",
            });

            await mediator.Send(new EditTicketTitleCommand
            {
                Title = "New title",
                RaisedByUser = "Balazs2",
                TicketId = id
            });

            await mediator.Send(new EditTicketDescriptionCommand
            {
                Description = "New description",
                RaisedByUser = "Balazs2",
                TicketId = id
            });

            await mediator.Send(new ChangeTicketStatusCommand
            {
                NewStatus = Domain.Common.TicketStatuses.InProgress,
                TicketId = id,
                RaisedByUser = "Balazs2"
            });

            await mediator.Send(new AddTicketTagsCommand
            {
                Tags = new[] { "Dev" },
                TicketId = id,
                RaisedByUser = "Balazs"
            });

            await mediator.Send(new AddTicketTagsCommand
            {
                Tags = new[] { "PoC", "Backend" },
                TicketId = id,
                RaisedByUser = "Balazs"
            });

            await mediator.Send(new RemoveTicketTagsCommand
            {
                Tags = new[] { "Dev" },
                TicketId = id,
                RaisedByUser = "Balazs"
            });

            await mediator.Send(new AddTicketTagsCommand
            {
                Tags = new[] { "QA" },
                TicketId = id,
                RaisedByUser = "Balazs"
            });

            await mediator.Send(new AddTicketLinksCommand
            {
                Links = new[]
                {
                    new TicketLinkDTO
                    {
                        LinkType = Domain.Common.TicketLinkTypes.PartOf,
                        TargetTicketId = id2,
                    },
                    new TicketLinkDTO
                    {
                        LinkType = Domain.Common.TicketLinkTypes.RelatedTo,
                        TargetTicketId = id2,
                    },
                },
                TicketId = id,
                RaisedByUser = "Balazs"
            });

            await mediator.Send(new AddTicketLinksCommand
            {
                Links = new[]
                {
                    new TicketLinkDTO
                    {
                        LinkType = Domain.Common.TicketLinkTypes.BlockedBy,
                        TargetTicketId = id2,
                    },
                    new TicketLinkDTO
                    {
                        LinkType = Domain.Common.TicketLinkTypes.RelatedTo,
                        TargetTicketId = id2,
                    },
                },
                TicketId = id,
                RaisedByUser = "Balazs"
            });

            return Ok();
        }
    }
}