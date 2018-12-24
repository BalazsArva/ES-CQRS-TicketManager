using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.DTOs.Notifications;

namespace TicketManager.WebAPI.Services.CommandHandlers
{
    public class EditTicketTitleCommandHandler : IRequestHandler<EditTicketTitleCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<EditTicketTitleCommand> validator;

        public EditTicketTitleCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, IValidator<EditTicketTitleCommand> validator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<Unit> Handle(EditTicketTitleCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            using (var context = eventsContextFactory.CreateContext())
            {
                context.TicketTitleChangedEvents.Add(new TicketTitleChangedEvent
                {
                    CausedBy = request.RaisedByUser,
                    TicketCreatedEventId = request.TicketId,
                    Title = request.Title
                });

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            await mediator.Publish(new TicketTitleChangedNotification(request.TicketId), cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }
    }
}