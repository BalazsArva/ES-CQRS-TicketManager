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
    public class EditTicketDetailsCommandHandler : IRequestHandler<EditTicketDetailsCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly EditTicketDetailsCommandValidator editTicketDetailsCommandValidator;

        public EditTicketDetailsCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, EditTicketDetailsCommandValidator editTicketDetailsCommandValidator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.editTicketDetailsCommandValidator = editTicketDetailsCommandValidator ?? throw new ArgumentNullException(nameof(editTicketDetailsCommandValidator));
        }

        public async Task<Unit> Handle(EditTicketDetailsCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await editTicketDetailsCommandValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            using (var context = eventsContextFactory.CreateContext())
            {
                context.TicketDetailsChangedEvents.Add(new TicketDetailsChangedEvent
                {
                    CausedBy = request.Editor,
                    Description = request.Description,
                    TicketCreatedEventId = request.TicketId,
                    Title = request.Title,
                    UtcDateRecorded = DateTime.UtcNow
                });

                await context.SaveChangesAsync();
            }

            await mediator.Publish(new TicketDetailsChangedNotification(request.TicketId));

            return Unit.Value;
        }
    }
}