using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.DTOs.Notifications;
using TicketManager.WebAPI.Validation;

namespace TicketManager.WebAPI.Services.CommandHandlers
{
    public class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, int>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly CreateTicketCommandValidator createTicketCommandValidator;

        public CreateTicketCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, CreateTicketCommandValidator createTicketCommandValidator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.createTicketCommandValidator = createTicketCommandValidator ?? throw new ArgumentNullException(nameof(createTicketCommandValidator));
        }

        public async Task<int> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await createTicketCommandValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            int ticketId;
            using (var context = eventsContextFactory.CreateContext())
            {
                var now = DateTime.UtcNow;

                var ticketCreatedEvent = new TicketCreatedEvent
                {
                    CausedBy = request.Creator,
                    UtcDateRecorded = now
                };

                context.TicketCreatedEvents.Add(ticketCreatedEvent);
                context.TicketDetailsChangedEvents.Add(new TicketDetailsChangedEvent
                {
                    CausedBy = request.Creator,
                    Description = request.Description,
                    Priority = request.Priority,
                    TicketCreatedEvent = ticketCreatedEvent,
                    TicketType = request.TicketType,
                    Title = request.Title,
                    UtcDateRecorded = now
                });
                context.TicketStatusChangedEvents.Add(new TicketStatusChangedEvent
                {
                    TicketCreatedEvent = ticketCreatedEvent,
                    CausedBy = request.Creator,
                    TicketStatus = TicketStatus.NotStarted,
                    UtcDateRecorded = now
                });
                context.TicketAssignedEvents.Add(new TicketAssignedEvent
                {
                    AssignedTo = null,
                    CausedBy = request.Creator,
                    TicketCreatedEvent = ticketCreatedEvent,
                    UtcDateRecorded = now
                });

                await context.SaveChangesAsync();

                ticketId = ticketCreatedEvent.Id;
            }

            await mediator.Publish(new TicketCreatedNotification(ticketId));

            return ticketId;
        }
    }
}