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
    public class CancelTicketInvolvementCommandHandler : IRequestHandler<CancelTicketInvolvementCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<CancelTicketInvolvementCommand> validator;

        public CancelTicketInvolvementCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, IValidator<CancelTicketInvolvementCommand> validator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<Unit> Handle(CancelTicketInvolvementCommand request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            using (var context = eventsContextFactory.CreateContext())
            {
                context.TicketUserInvolvementCancelledEvents.Add(new TicketUserInvolvementCancelledEvent
                {
                    AffectedUser = request.CancelInvolvementFor,
                    CausedBy = request.RaisedByUser,
                    TicketCreatedEventId = request.TicketId
                });

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            await mediator.Publish(new TicketUserInvolvementCancelledNotification(request.TicketId), cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }
    }
}