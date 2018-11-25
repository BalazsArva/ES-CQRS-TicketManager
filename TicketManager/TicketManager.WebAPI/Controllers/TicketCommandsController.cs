using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
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
                Creator = "Balazs",
                Description = "Test",
                Priority = Domain.Common.Priority.Lowest,
                TicketType = Domain.Common.TicketType.Task,
                Title = "Test"
            });

            id = 26023;

            await mediator.Send(new AssignTicketCommand
            {
                TicketId = id,
                Assigner = "Balazs2",
                AssignTo = "Balazs",
            });

            await mediator.Send(new EditTicketDetailsCommand
            {
                Description = "New description",
                Title = "New title",
                Editor = "Balazs2",
                Priority = Domain.Common.Priority.Highest,
                TicketId = id,
                TicketType = Domain.Common.TicketType.Bug
            });

            await mediator.Send(new ChangeTicketStatusCommand
            {
                NewStatus = Domain.Common.TicketStatus.InProgress,
                TicketId = id,
                User = "Balazs2"
            });

            await mediator.Send(new AddTicketTagCommand
            {
                Tag = "Dev",
                TicketId = id,
                User = "Balazs"
            });

            await mediator.Send(new AddTicketTagCommand
            {
                Tag = "PoC",
                TicketId = id,
                User = "Balazs"
            });

            await mediator.Send(new RemoveTicketTagCommand
            {
                Tag = "Dev",
                TicketId = id,
                User = "Balazs"
            });

            await mediator.Send(new AddTicketTagCommand
            {
                Tag = "QA",
                TicketId = id,
                User = "Balazs"
            });

            return Ok();
        }
    }
}