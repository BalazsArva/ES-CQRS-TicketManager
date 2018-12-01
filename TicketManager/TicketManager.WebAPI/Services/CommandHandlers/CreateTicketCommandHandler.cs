using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.DTOs.Notifications;
using TicketManager.WebAPI.Validation.CommandValidators;

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
                var ticketCreatedEvent = new TicketCreatedEvent { CausedBy = request.RaisedByUser };

                context.TicketCreatedEvents.Add(ticketCreatedEvent);
                context.TicketTitleChangedEvents.Add(new TicketTitleChangedEvent
                {
                    CausedBy = request.RaisedByUser,
                    TicketCreatedEvent = ticketCreatedEvent,
                    Title = request.Title
                });
                context.TicketDescriptionChangedEvents.Add(new TicketDescriptionChangedEvent
                {
                    CausedBy = request.RaisedByUser,
                    Description = request.Description,
                    TicketCreatedEvent = ticketCreatedEvent
                });
                context.TicketPriorityChangedEvents.Add(new TicketPriorityChangedEvent
                {
                    CausedBy = request.RaisedByUser,
                    Priority = request.Priority,
                    TicketCreatedEvent = ticketCreatedEvent
                });
                context.TicketTypeChangedEvents.Add(new TicketTypeChangedEvent
                {
                    CausedBy = request.RaisedByUser,
                    TicketType = request.TicketType,
                    TicketCreatedEvent = ticketCreatedEvent
                });
                context.TicketStatusChangedEvents.Add(new TicketStatusChangedEvent
                {
                    TicketCreatedEvent = ticketCreatedEvent,
                    CausedBy = request.RaisedByUser,
                    TicketStatus = request.TicketStatus
                });
                context.TicketAssignedEvents.Add(new TicketAssignedEvent
                {
                    AssignedTo = request.AssignTo,
                    CausedBy = request.RaisedByUser,
                    TicketCreatedEvent = ticketCreatedEvent
                });

                await context.SaveChangesAsync();

                ticketId = ticketCreatedEvent.Id;
            }

            await mediator.Publish(new TicketCreatedNotification(ticketId));

            return ticketId;
        }
    }
}