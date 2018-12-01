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
    public class EditTicketDescriptionCommandHandler : IRequestHandler<EditTicketDescriptionCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<EditTicketDescriptionCommand> validator;

        public EditTicketDescriptionCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, IValidator<EditTicketDescriptionCommand> validator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<Unit> Handle(EditTicketDescriptionCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            using (var context = eventsContextFactory.CreateContext())
            {
                context.TicketDescriptionChangedEvents.Add(new TicketDescriptionChangedEvent
                {
                    CausedBy = request.RaisedByUser,
                    Description = request.Description,
                    TicketCreatedEventId = request.TicketId
                });

                await context.SaveChangesAsync();
            }

            await mediator.Publish(new TicketDescriptionChangedNotification(request.TicketId));

            return Unit.Value;
        }
    }
}