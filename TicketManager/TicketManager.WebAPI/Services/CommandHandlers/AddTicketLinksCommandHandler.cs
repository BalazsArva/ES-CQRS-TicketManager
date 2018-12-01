﻿using System;
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
    public class AddTicketLinksCommandHandler : IRequestHandler<AddTicketLinksCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly AddTicketLinksCommandValidator addTicketLinksCommandValidator;

        public AddTicketLinksCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, AddTicketLinksCommandValidator addTicketLinksCommandValidator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.addTicketLinksCommandValidator = addTicketLinksCommandValidator ?? throw new ArgumentNullException(nameof(addTicketLinksCommandValidator));
        }

        public async Task<Unit> Handle(AddTicketLinksCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await addTicketLinksCommandValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            using (var context = eventsContextFactory.CreateContext())
            {
                foreach (var ticketLink in request.Links)
                {
                    var ticketLinkChangedEvent = new TicketLinkChangedEvent
                    {
                        CausedBy = request.RaisedByUser,
                        LinkType = ticketLink.LinkType,
                        SourceTicketCreatedEventId = request.TicketId,
                        TargetTicketCreatedEventId = ticketLink.TargetTicketId,
                        ConnectionIsActive = true
                    };

                    // TODO: Consider whether: there should be validation that the link is not yet established, OR simply ignore and get the distinct links on the query level.
                    context.TicketLinkChangedEvents.Add(ticketLinkChangedEvent);
                }

                await context.SaveChangesAsync();
            }

            await mediator.Publish(new TicketLinksAddedNotification(request.TicketId));

            return Unit.Value;
        }
    }
}