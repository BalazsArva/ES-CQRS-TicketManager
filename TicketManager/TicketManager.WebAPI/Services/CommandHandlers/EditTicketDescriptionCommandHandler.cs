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
    public class EditTicketDescriptionCommandHandler : IRequestHandler<EditTicketDescriptionCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly EditTicketDescriptionCommandValidator editTicketDescriptionCommandValidator;

        public EditTicketDescriptionCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, EditTicketDescriptionCommandValidator editTicketDescriptionCommandValidator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.editTicketDescriptionCommandValidator = editTicketDescriptionCommandValidator ?? throw new ArgumentNullException(nameof(editTicketDescriptionCommandValidator));
        }

        public async Task<Unit> Handle(EditTicketDescriptionCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await editTicketDescriptionCommandValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            using (var context = eventsContextFactory.CreateContext())
            {
                context.TicketDescriptionChangedEvents.Add(new TicketDescriptionChangedEvent
                {
                    CausedBy = request.Editor,
                    Description = request.Description,
                    TicketCreatedEventId = request.TicketId,
                    UtcDateRecorded = DateTime.UtcNow
                });

                await context.SaveChangesAsync();
            }

            await mediator.Publish(new TicketDescriptionChangedNotification(request.TicketId));

            return Unit.Value;
        }
    }
}