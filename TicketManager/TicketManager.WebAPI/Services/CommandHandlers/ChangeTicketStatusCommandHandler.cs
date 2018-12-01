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
    public class ChangeTicketStatusCommandHandler : IRequestHandler<ChangeTicketStatusCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<ChangeTicketStatusCommand> validator;

        public ChangeTicketStatusCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, IValidator<ChangeTicketStatusCommand> validator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<Unit> Handle(ChangeTicketStatusCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            using (var context = eventsContextFactory.CreateContext())
            {
                context.TicketStatusChangedEvents.Add(new TicketStatusChangedEvent
                {
                    CausedBy = request.RaisedByUser,
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