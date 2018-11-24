using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.DTOs.Notifications;
using TicketManager.WebAPI.Validation;

namespace TicketManager.WebAPI.Services.CommandHandlers
{
    public class AssignTicketCommandHandler : IRequestHandler<AssignTicketCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly AssignTicketCommandValidator assignTicketCommandValidator;

        public AssignTicketCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, AssignTicketCommandValidator assignTicketCommandValidator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.assignTicketCommandValidator = assignTicketCommandValidator ?? throw new ArgumentNullException(nameof(assignTicketCommandValidator));
        }

        public async Task<Unit> Handle(AssignTicketCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await assignTicketCommandValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            using (var context = eventsContextFactory.CreateContext())
            {
                context.TicketAssignedEvents.Add(new TicketAssignedEvent
                {
                    AssignedTo = request.AssignTo,
                    CausedBy = request.Assigner,
                    TicketCreatedEventId = request.TicketId,

                    // TODO: Maybe should populate this DB side. In a real scenario, the clocks of different nodes can be different and if we filter on date when
                    // eg. updating a snapshot, there might be intermediate events whose values are earlier but get inserted to the DB slightly later. In this case,
                    // if we filter on UtcDateRecorded > LatestSnapshot.UtcLastUpdated, some values will be skipped which hurts if the current value is an aggregate
                    // of all the earlier values.
                    UtcDateRecorded = DateTime.UtcNow
                });

                await context.SaveChangesAsync();
            }

            await mediator.Publish(new TicketAssignedNotification(request.TicketId));

            return Unit.Value;
        }
    }
}