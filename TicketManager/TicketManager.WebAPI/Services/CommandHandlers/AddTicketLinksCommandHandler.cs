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
    public class AddTicketLinksCommandHandler : IRequestHandler<AddTicketLinksCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<AddTicketLinksCommand> validator;

        public AddTicketLinksCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, IValidator<AddTicketLinksCommand> validator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<Unit> Handle(AddTicketLinksCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            using (var context = eventsContextFactory.CreateContext())
            {
                foreach (var ticketLink in request.Links)
                {
                    context.TicketLinkChangedEvents.Add(new TicketLinkChangedEvent
                    {
                        CausedBy = request.RaisedByUser,
                        LinkType = ticketLink.LinkType,
                        SourceTicketCreatedEventId = request.TicketId,
                        TargetTicketCreatedEventId = ticketLink.TargetTicketId,
                        ConnectionIsActive = true
                    });
                }

                await context.SaveChangesAsync();
            }

            await mediator.Publish(new TicketLinksAddedNotification(request.TicketId));

            return Unit.Value;
        }
    }
}