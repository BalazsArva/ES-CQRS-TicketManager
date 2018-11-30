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
                    TicketCreatedEventId = request.TicketId
                });

                await context.SaveChangesAsync();
            }

            await mediator.Publish(new TicketAssignedNotification(request.TicketId));

            return Unit.Value;
        }
    }
}