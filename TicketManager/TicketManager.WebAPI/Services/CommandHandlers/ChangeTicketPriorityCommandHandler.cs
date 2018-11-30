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
    public class ChangeTicketPriorityCommandHandler : IRequestHandler<ChangeTicketPriorityCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly ChangeTicketPriorityCommandValidator changeTicketPriorityCommandValidator;

        public ChangeTicketPriorityCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, ChangeTicketPriorityCommandValidator changeTicketPriorityCommandValidator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.changeTicketPriorityCommandValidator = changeTicketPriorityCommandValidator ?? throw new ArgumentNullException(nameof(changeTicketPriorityCommandValidator));
        }

        public async Task<Unit> Handle(ChangeTicketPriorityCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await changeTicketPriorityCommandValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            using (var context = eventsContextFactory.CreateContext())
            {
                context.TicketPriorityChangedEvents.Add(new TicketPriorityChangedEvent
                {
                    CausedBy = request.User,
                    TicketCreatedEventId = request.TicketId,
                    Priority = request.Priority,
                    UtcDateRecorded = DateTime.UtcNow
                });

                await context.SaveChangesAsync();
            }

            await mediator.Publish(new TicketPriorityChangedNotification(request.TicketId));

            return Unit.Value;
        }
    }
}