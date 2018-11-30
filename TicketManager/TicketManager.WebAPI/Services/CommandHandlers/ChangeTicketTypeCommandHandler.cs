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
    public class ChangeTicketTypeCommandHandler : IRequestHandler<ChangeTicketTypeCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly ChangeTicketTypeCommandValidator changeTicketTypeCommandValidator;

        public ChangeTicketTypeCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, ChangeTicketTypeCommandValidator changeTicketTypeCommandValidator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.changeTicketTypeCommandValidator = changeTicketTypeCommandValidator ?? throw new ArgumentNullException(nameof(changeTicketTypeCommandValidator));
        }

        public async Task<Unit> Handle(ChangeTicketTypeCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await changeTicketTypeCommandValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            using (var context = eventsContextFactory.CreateContext())
            {
                context.TicketTypeChangedEvents.Add(new TicketTypeChangedEvent
                {
                    CausedBy = request.User,
                    TicketCreatedEventId = request.TicketId,
                    TicketType = request.TicketType,
                    UtcDateRecorded = DateTime.UtcNow
                });

                await context.SaveChangesAsync();
            }

            await mediator.Publish(new TicketTypeChangedNotification(request.TicketId));

            return Unit.Value;
        }
    }
}