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
    public class ChangeTicketStatusCommandHandler : IRequestHandler<ChangeTicketStatusCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly ChangeTicketStatusCommandValidator changeTicketStatusCommandValidator;

        public ChangeTicketStatusCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, ChangeTicketStatusCommandValidator changeTicketStatusCommandValidator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.changeTicketStatusCommandValidator = changeTicketStatusCommandValidator ?? throw new ArgumentNullException(nameof(changeTicketStatusCommandValidator));
        }

        public async Task<Unit> Handle(ChangeTicketStatusCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await changeTicketStatusCommandValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            using (var context = eventsContextFactory.CreateContext())
            {
                context.TicketStatusChangedEvents.Add(new TicketStatusChangedEvent
                {
                    CausedBy = request.User,
                    TicketCreatedEventId = request.TicketId,
                    TicketStatus = request.NewStatus
                });

                await context.SaveChangesAsync();
            }

            await mediator.Publish(new TicketStatusChangedNotification(request.TicketId));

            return Unit.Value;
        }
    }
}