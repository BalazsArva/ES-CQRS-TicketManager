﻿using System;
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
    public class ChangeTicketStoryPointsCommandHandler : IRequestHandler<ChangeTicketStoryPointsCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<ChangeTicketStoryPointsCommand> validator;

        public ChangeTicketStoryPointsCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, IValidator<ChangeTicketStoryPointsCommand> validator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<Unit> Handle(ChangeTicketStoryPointsCommand request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            using (var context = eventsContextFactory.CreateContext())
            {
                context.TicketStoryPointsChangedEvents.Add(new TicketStoryPointsChangedEvent
                {
                    StoryPoints = request.StoryPoints,
                    CausedBy = request.RaisedByUser,
                    TicketCreatedEventId = request.TicketId
                });

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            await mediator.Publish(new TicketStoryPointsChangedNotification(request.TicketId), cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }
    }
}