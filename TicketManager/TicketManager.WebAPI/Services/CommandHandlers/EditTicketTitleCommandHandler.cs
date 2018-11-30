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
    public class EditTicketTitleCommandHandler : IRequestHandler<EditTicketTitleCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly EditTicketTitleCommandValidator editTicketTitleCommandValidator;

        public EditTicketTitleCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, EditTicketTitleCommandValidator editTicketTitleCommandValidator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.editTicketTitleCommandValidator = editTicketTitleCommandValidator ?? throw new ArgumentNullException(nameof(editTicketTitleCommandValidator));
        }

        public async Task<Unit> Handle(EditTicketTitleCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await editTicketTitleCommandValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            using (var context = eventsContextFactory.CreateContext())
            {
                context.TicketTitleChangedEvents.Add(new TicketTitleChangedEvent
                {
                    CausedBy = request.Editor,
                    TicketCreatedEventId = request.TicketId,
                    Title = request.Title,
                    UtcDateRecorded = DateTime.UtcNow
                });

                await context.SaveChangesAsync();
            }

            await mediator.Publish(new TicketTitleChangedNotification(request.TicketId));

            return Unit.Value;
        }
    }
}