using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.DTOs.Notifications;
using TicketManager.WebAPI.Services.Providers;

namespace TicketManager.WebAPI.Services.CommandHandlers
{
    public class ChangeTicketTitleCommandHandler : IRequestHandler<ChangeTicketTitleCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<ChangeTicketTitleCommand> validator;
        private readonly ICorrelationIdProvider correlationIdProvider;

        public ChangeTicketTitleCommandHandler(ICorrelationIdProvider correlationIdProvider, IMediator mediator, IEventsContextFactory eventsContextFactory, IValidator<ChangeTicketTitleCommand> validator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            this.correlationIdProvider = correlationIdProvider ?? throw new ArgumentNullException(nameof(correlationIdProvider));
        }

        public async Task<Unit> Handle(ChangeTicketTitleCommand request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            using (var context = eventsContextFactory.CreateContext())
            {
                context.TicketTitleChangedEvents.Add(new TicketTitleChangedEvent
                {
                    CorrelationId = correlationIdProvider.GetCorrelationId(),
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